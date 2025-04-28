using UnityEngine;

namespace EmberAI.Attributes
{
    /// <summary>
    /// Calls the specified method whenever the field’s value changes in the Inspector.
    /// </summary>
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string MethodName { get; }

        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}