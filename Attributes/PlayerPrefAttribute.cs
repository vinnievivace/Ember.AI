namespace EmberAI.Attributes
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field)]
    public class PlayerPrefAttribute : PropertyAttribute
    {
        /// <summary>
        /// Optional custom key. If null or empty, the field’s name is used.
        /// </summary>
        public string Key { get; }

        public PlayerPrefAttribute(string key = null)
        {
            Key = key;
        }
    }

}