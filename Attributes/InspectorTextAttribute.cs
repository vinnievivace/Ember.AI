using UnityEngine;

namespace EmberAI.Attributes
{
    

    /// <summary>
    /// Display a field’s value as a read-only text (no label) in the Inspector.
    /// Use #pragma warning disable CS0414 to avoid the "assigned but its value is never used" warning in Unity.
    /// </summary>
    public class InspectorTextAttribute : PropertyAttribute { }

}