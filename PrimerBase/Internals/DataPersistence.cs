using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Primer
{
    public static class DataPersistence
    {
        static readonly string resources = Path.Combine(Application.dataPath, "Resources");

        public static void WriteToFile<T>(T data, string filePath) {
            using var stream = File.Open(filePath, FileMode.Create);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }

        public static T ReadFromFile<T>(string filePath, T defaultValue = default) {
            try {
                using var stream = File.Open(filePath, FileMode.Open);
                var formatter = new BinaryFormatter();
                var deserialized = formatter.Deserialize(stream);
                return (T)deserialized;
            }
            catch {
                return defaultValue;
            }
        }

        public static void SaveToResources<T>(T data, string fileName) {
            if (!Directory.Exists(resources)) {
                Directory.CreateDirectory(resources);
            }

            var filePath = Path.Combine(resources, fileName);
            WriteToFile(data, filePath);
        }

        public static T LoadFromResources<T>(string fileName, T defaultValue = default) {
            var filePath = Path.Combine(resources, fileName);
            return ReadFromFile(filePath, defaultValue);
        }
    }
}
