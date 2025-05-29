using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor
{
    /// <summary>
    /// Various Editor Layout Utilities used by EmberAI custom editors to implement custom attributes/drawers.
    /// </summary>
    public static class EditorLayoutUtils
    {
        private class RowState
        {
            public Rect RowRect;
            public float CurrentX;
        }

        private static readonly Dictionary<int, RowState> RowStates = new Dictionary<int, RowState>();

        /// <summary>
        /// Call at the start of OnInspectorGUI() to reset row layout state.
        /// </summary>
        public static void BeginRows()
        {
            RowStates.Clear();
        }

        /// <summary>
        /// Returns a sub-rect for the given row index, sized as widthPercent of the inspector width.
        /// </summary>
        public static Rect GetButtonRect(int row, float widthPercent, float height = 50f, float spaceAbove = 5f, float spaceBelow = 0f)
        {
            widthPercent = Mathf.Clamp01(widthPercent);

            if (!RowStates.TryGetValue(row, out var rowState))
            {
                rowState = new RowState();
                float totalHeight = height + spaceAbove + spaceBelow;
                rowState.RowRect = EditorGUILayout.GetControlRect(false, totalHeight);
                rowState.CurrentX = rowState.RowRect.x;
                RowStates[row] = rowState;
            }

            float subWidth = rowState.RowRect.width * widthPercent;
            Rect rect = new Rect(rowState.CurrentX, rowState.RowRect.y + spaceAbove, subWidth, height);
            rowState.CurrentX += subWidth;
            return rect;
        }

        public static GUIStyle GetButtonStyle(FontStyle style = FontStyle.Bold)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = style
            };
        }

        /// <summary>
        /// Draws buttons for methods on 'targetType' decorated with [ButtonGroup], invoking them on each target object.
        /// </summary>
        public static void DrawButtonGroups(Type targetType, UnityEngine.Object[] targets)
        {
            // Collect all instance methods with ButtonGroupAttribute
            var methods = targetType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ButtonGroupAttribute>() != null)
                .ToArray();

            if (methods.Length == 0)
                return;

            EditorGUILayout.Space(8);

            // Group methods by their GroupID
            var groups = methods.GroupBy(m => m.GetCustomAttribute<ButtonGroupAttribute>().GroupID);

            foreach (var group in groups)
            {
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                BeginRows();

                int count = group.Count();
                foreach (var method in group)
                {
                    var attr = method.GetCustomAttribute<ButtonGroupAttribute>();
                    float width = 1f / count;
                    var content = new GUIContent(attr.Label, attr.Tooltip);

                    if (GUI.Button(GetButtonRect(0, width), content, GetButtonStyle()))
                    {
                        foreach (var t in targets)
                        {
                            // Invoke non-static method on each target instance
                            method.Invoke(t, null);
                        }
                    }
                }

                EditorGUILayout.Space(6);
            }
        }
    }
}