using UnityEngine;

namespace EmberAI.Attributes
{
    /// <summary>
    /// Organise groups of fields in the Inspector.
    /// </summary>
    public class BoxGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; private set; }

        public BoxGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}