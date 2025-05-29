using System.Collections.Generic;
using System.Reflection;
using EmberAI.Attributes;
using EmberAI.Core;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor
{
    /// <summary>
    /// Custom Editor for BaseData ScriptableObjects.
    /// </summary>
    [CustomEditor(typeof(BaseData), true)]
    public class DataEditor : UnityEditor.Editor
    {
        private BaseData _dataTarget;
        private Dictionary<string, List<SerializedProperty>> _groupedProperties;
        private List<SerializedProperty> _ungroupedProperties;

        private void OnEnable()
        {
            CacheProperties();
        }

        public override void OnInspectorGUI()
        {
            _dataTarget = (BaseData)target;
            _dataTarget.Initialize();

            DrawHeaderRow();
            DrawProperties();

            // Render any [ButtonGroup] methods for BaseData
            EditorLayoutUtils.DrawButtonGroups(target.GetType(), targets);
        }

        private void DrawHeaderRow()
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float height = 40f;
            Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EmberEditor.HeaderColor);

            // Logo
            Texture2D logo = AssetDatabase.LoadAssetAtPath<Texture2D>(EmberEditor.GetLogoPath());
            if (logo == null)
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/{EmberEditor.PackageName}/Logo.png");
            if (logo != null)
            {
                Rect logoRect = new Rect(rect.x + 4, rect.y + 4, height - 8, height - 8);
                GUI.DrawTexture(logoRect, logo, ScaleMode.ScaleToFit);
            }

            // Title
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleLeft
            };
            Rect labelRect = new Rect(rect.x + height, rect.y, rect.width - height, rect.height);
            GUI.Label(labelRect, $"Data: {_dataTarget.name} ({_dataTarget.GetType().Name})", style);

            EditorGUI.indentLevel = oldIndent;
        }

        private void DrawProperties()
        {
            serializedObject.Update();

            // Draw grouped properties
            foreach (var kvp in _groupedProperties)
            {
                EditorGUILayout.BeginVertical("box");
                if (!string.IsNullOrEmpty(kvp.Key))
                {
                    EditorGUILayout.LabelField(kvp.Key, EditorStyles.boldLabel);
                    EditorGUILayout.Space(2);
                }
                EditorGUI.indentLevel++;
                foreach (var prop in kvp.Value)
                    EditorGUILayout.PropertyField(prop, true);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            // Draw ungrouped properties
            foreach (var prop in _ungroupedProperties)
                EditorGUILayout.PropertyField(prop, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void CacheProperties()
        {
            _groupedProperties = new Dictionary<string, List<SerializedProperty>>();
            _ungroupedProperties = new List<SerializedProperty>();

            SerializedProperty iterator = serializedObject.GetIterator();
            if (!iterator.NextVisible(true))
                return;

            do
            {
                if (iterator.propertyPath == "m_Script")
                    continue;

                var field = target.GetType().GetField(iterator.name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                var boxAttr = field?.GetCustomAttribute<BoxGroupAttribute>();
                if (boxAttr != null)
                {
                    if (!_groupedProperties.ContainsKey(boxAttr.GroupName))
                        _groupedProperties[boxAttr.GroupName] = new List<SerializedProperty>();
                    _groupedProperties[boxAttr.GroupName].Add(iterator.Copy());
                }
                else
                {
                    _ungroupedProperties.Add(iterator.Copy());
                }
            }
            while (iterator.NextVisible(false));
        }
    }
}