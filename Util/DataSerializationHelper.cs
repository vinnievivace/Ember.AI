using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EmberAI.Util
{
    public static class DataSerializationHelper
    {
        public static T LoadAndDeserializeJson<T>(string fullFilePath)
        {
            string json = File.ReadAllText(fullFilePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SerializeJsonAndSaveData<T>(T data, string fullFilePath, Formatting formatting = Formatting.None)
        {
            string json = JsonConvert.SerializeObject(data, formatting);
            File.WriteAllText(fullFilePath, json);
        }
        
        public static T DeepCloneWithJson<T>(this T obj)
        {
            string json = JsonConvert.SerializeObject(obj);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}