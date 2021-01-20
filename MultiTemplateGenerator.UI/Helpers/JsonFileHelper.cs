using System.IO;
using MultiTemplateGenerator.Lib;
using Newtonsoft.Json;

namespace MultiTemplateGenerator.UI.Helpers
{
    public static class JsonFileHelper
    {
        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };

        public static T ReadJsonFile<T>(string fileName) where T : class
        {
            if (!File.Exists(fileName))
                return null;

            string content = fileName.ReadFileContent();
            return JsonConvert.DeserializeObject<T>(content, JsonSettings);
        }

        public static string GetJson<T>(T objectToSave)
        {
            return JsonConvert.SerializeObject(objectToSave, Formatting.Indented, JsonSettings);
        }

        public static void SaveJsonFile<T>(T objectToSave, string fileName)
        {
            string content = GetJson(objectToSave);
            fileName.WriteFileContent(content);
        }
    }
}
