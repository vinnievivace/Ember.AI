using System;
using UnityEngine;

namespace EmberAI.Core.Util
{
    public class ScriptableObjectUtil
    {
        public static string ToJSON(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is null.");

            return JsonUtility.ToJson(scriptableObject);
        }

        public static T FromJSON<T>(string jsonString) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(jsonString))
                throw new ArgumentException("Invalid JSON string.", nameof(jsonString));

            T instance = ScriptableObject.CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(jsonString, instance);
			
            return instance;
        }
        
        public static bool Compare(ScriptableObject obj1, ScriptableObject obj2)
        {
            if (obj1 == null || obj2 == null)
                throw new ArgumentNullException("One or both of the ScriptableObject arguments are null.");

            if (obj1.GetType() != obj2.GetType())
                throw new ArgumentException("Both objects must be of the same type to compare.");

            string json1 = JsonUtility.ToJson(obj1);
            string json2 = JsonUtility.ToJson(obj2);

            return json1 == json2;
        }
    }
}