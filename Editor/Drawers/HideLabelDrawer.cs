using EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(HideLabelAttribute))]
    public class HideLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Create a GUIContent with an empty string or whitespace, allows for foldout arrow for nested objects
            GUIContent foldoutContent = GUIContent.none; 

            EditorGUI.PropertyField(position, property, foldoutContent, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, GUIContent.none, true);
        }
    }
}