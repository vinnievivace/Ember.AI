using EmberAI.Attributes;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InspectorTextAttribute))]
    public class InspectorTextDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string displayValue = GetStringValue(property);

            EditorGUI.LabelField(position, displayValue, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// Converts a SerializedProperty’s value to a displayable string. 
        /// </summary>
        private string GetStringValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.###");
                
                case SerializedPropertyType.String:
                    return property.stringValue;
                
                case SerializedPropertyType.Enum:
                    return property.enumDisplayNames[property.enumValueIndex];
                
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue ? property.objectReferenceValue.name : "None";
                
                default:
                    return property.displayName + " (Unsupported type)";
            }
        }
    }
}