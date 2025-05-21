using EmberAI.Attributes;
using UnityEngine;
using UnityEditor;

namespace EmberAI.Editor.Drawers
{
    
    [CustomPropertyDrawer(typeof(PlayerPrefAttribute))]
    public class PlayerPrefDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (PlayerPrefAttribute)attribute;
            // choose key: either the one passed in, or the field name
            string key = string.IsNullOrEmpty(attr.Key)
                ? property.name
                : attr.Key;

            // Load from PlayerPrefs into the serializedProperty before drawing
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    property.stringValue = PlayerPrefs.GetString(key, property.stringValue);
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = PlayerPrefs.GetInt(key, property.intValue);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = PlayerPrefs.GetFloat(key, property.floatValue);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = PlayerPrefs.GetInt(key, property.boolValue ? 1 : 0) == 1;
                    break;
                default:
                    EditorGUI.LabelField(position, label.text, $"Type '{property.propertyType}' not supported");
                    return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            // Draw the default field UI
            EditorGUI.PropertyField(position, property, label, true);

            if (EditorGUI.EndChangeCheck())
            {
                // On change, write back to PlayerPrefs
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        PlayerPrefs.SetString(key, property.stringValue);
                        break;
                    case SerializedPropertyType.Integer:
                        PlayerPrefs.SetInt(key, property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        PlayerPrefs.SetFloat(key, property.floatValue);
                        break;
                    case SerializedPropertyType.Boolean:
                        PlayerPrefs.SetInt(key, property.boolValue ? 1 : 0);
                        break;
                }
                PlayerPrefs.Save();
            }
            EditorGUI.EndProperty();
        }
    }
}