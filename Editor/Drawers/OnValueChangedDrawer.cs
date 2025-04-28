using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using EmberAI.Attributes;

namespace EmberAI.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedDrawer : PropertyDrawer
    {
        // Store old values per property, so they persist across OnGUI calls
        private static readonly System.Collections.Generic.Dictionary<string, object> OldValues = new ();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 1. Draw the property normally first
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);

            // 2. Check if the value changed
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();

                // 3. Compare new value to old value
                object newValue = GetPropertyValue(property);
                bool valueChanged = false;

                string key = property.serializedObject.targetObject.GetInstanceID() + "_" + property.propertyPath;

                // If we have an old value stored, compare
                if (OldValues.TryGetValue(key, out object oldValue))
                {
                    if (!Equals(newValue, oldValue))
                    {
                        valueChanged = true;
                    }
                    OldValues[key] = newValue; 
                }
                else
                {
                    // No old value stored yet, store it now
                    OldValues[key] = newValue;
                    valueChanged = true;
                }

                // 4. If changed, call the method
                if (valueChanged)
                {
                    OnValueChangedAttribute onValueChanged = (OnValueChangedAttribute)attribute;
                    CallMethod(property.serializedObject.targetObject, onValueChanged.MethodName);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private void CallMethod(UnityEngine.Object target, string methodName)
        {
            Type targetType = target.GetType();
            MethodInfo method = targetType.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
            {
                Debug.LogError($"OnValueChanged: Method '{methodName}' not found on {targetType}.");
                return;
            }

            if (method.GetParameters().Length == 0)
            {
                method.Invoke(target, null);
            }
            else
            {
                Debug.LogError($"OnValueChanged: Method '{methodName}' has parameters—must be parameterless.");
            }
        }

        private object GetPropertyValue(SerializedProperty prop)
        {
            UnityEngine.Object target = prop.serializedObject.targetObject;
            Type targetType = target.GetType();
            FieldInfo field = targetType.GetField(prop.name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            if (field != null)
            {
                return field.GetValue(target);
            }
            return null;
        }
    }
}