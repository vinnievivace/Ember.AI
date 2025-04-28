using EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ForceExpanded))]
    public class ForceExpandedDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = true;
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            SerializedProperty childProperty = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            bool enterChildren = true;
            float y = position.y;

            while (childProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(childProperty, endProperty))
            {
                // Skip script reference fields like 'm_Script' if this is a MonoBehaviour/ScriptableObject
                if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                {
                    continue;
                }

                float childHeight = EditorGUI.GetPropertyHeight(childProperty, null, true);
                Rect childRect = new Rect(position.x, y, position.width, childHeight);

                EditorGUI.PropertyField(childRect, childProperty, true);

                y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false; 
            }

            EditorGUI.indentLevel = oldIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0f;

            property.isExpanded = true;

            SerializedProperty childProperty = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            bool enterChildren = true;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            while (childProperty.NextVisible(enterChildren) && !SerializedProperty.EqualContents(childProperty, endProperty))
            {
                if (childProperty.name.Equals("m_Script", System.StringComparison.Ordinal))
                {
                    continue;
                }

                float childHeight = EditorGUI.GetPropertyHeight(childProperty, null, true);
                totalHeight += childHeight + spacing;

                enterChildren = false;
            }
            
            totalHeight -= spacing;
            
            if (totalHeight < 0f) totalHeight = EditorGUIUtility.singleLineHeight;

            return totalHeight;
        }
    }
}