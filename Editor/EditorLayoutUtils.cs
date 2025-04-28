using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace EmberAI.Editor
{
    /// <summary>
    /// Various Editor Layout Utilities used by <see cref="BlockEditor"/> to implement custom attributes / drawers.
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
        /// Should be called at the start of OnInspectorGUI(), to reset for a fresh layout pass.
        /// </summary>
        public static void BeginRows()
        {
            RowStates.Clear();
        }

        /// <summary>
        /// Returns a sub-Rect sized to (widthPercent of the Inspector width) x (height),
        /// placed in the specified 'row'. Optional spaceAbove/spaceBelow are included  in the same row allocation, but sub-Rect excludes them.
        /// 
        /// Usage:
        ///   Rect r1 = EditorLayoutUtils.GetButtonRect(0, 0.5f);
        ///   Rect r2 = EditorLayoutUtils.GetButtonRect(0, 0.5f);
        ///   => two side-by-side controls in row 0
        /// </summary>
        public static Rect GetButtonRect(int row, float widthPercent, float height = 50f, float spaceAbove = 5f, float spaceBelow = 0f)
        {
            widthPercent = Mathf.Clamp01(widthPercent);

            // If we've never requested this row before, allocate it now
            if (!RowStates.TryGetValue(row, out RowState rowState))
            {
                rowState = new RowState();

                float totalRowHeight = height + spaceAbove + spaceBelow;
                
                rowState.RowRect = EditorGUILayout.GetControlRect(false, totalRowHeight);
                rowState.CurrentX = rowState.RowRect.x; 
                RowStates[row] = rowState;
            }

            // The rowRect was already allocated on the first call to this row.
            float subRectWidth = rowState.RowRect.width * widthPercent;
            Rect subRect = new Rect(
                x:      rowState.CurrentX,
                y:      rowState.RowRect.y + spaceAbove, 
                width:  subRectWidth,
                height: height
            );

            rowState.CurrentX += subRectWidth;

            return subRect;
        }
        
        public static GUIStyle GetButtonStyle(FontStyle style = FontStyle.Bold)
        {
            GUIStyle boldButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = style
            };

            return boldButtonStyle;
        }
    }
}

