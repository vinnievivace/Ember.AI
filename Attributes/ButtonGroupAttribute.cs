namespace EmberAI.Attributes
{
    using System;

    namespace EmberAI.Attributes
    {
        /// <summary>
        /// Marks a method to be drawn as a button in the inspector, grouped by GroupID.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public class ButtonGroupAttribute : Attribute
        {
            public string GroupID { get; }
            public string Label { get; }
            public string Tooltip { get; }

            public ButtonGroupAttribute(string groupID, string label, string tooltip = "")
            {
                GroupID = groupID;
                Label = label;
                Tooltip = tooltip;
            }
        }
    }
}