using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Primer
{
    public class FileStorage<T>
    {
        readonly string filename;
        readonly T initialValue;

        public FileStorage(string filename, T initialValue = default) {
            this.filename = filename;
            this.initialValue = initialValue;
        }

        public void Write(T data) =>
            SaveToResources(data, filename);

        public T Read() =>
            LoadFromResources(filename, initialValue);


        #region Static methods
        static readonly string resources = Path.Combine(Application.dataPath, "Resources");

        public static void Write(T data, string filePath) {
            using var stream = File.Open(filePath, FileMode.Create);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }

        public static T Read(string filePath, T defaultValue = default) {
            try {
                using var stream = File.Open(filePath, FileMode.Open);
                var formatter = new BinaryFormatter();
                var deserialized = formatter.Deserialize(stream);
                return (T)deserialized;
            }
            catch (Exception ex) {
                Debug.Log($"Unable to deserialize {filePath}:\n{ex}");
                return defaultValue;
            }
        }

        public static void SaveToResources(T data, string fileName) {
            if (!Directory.Exists(resources)) {
                Directory.CreateDirectory(resources);
            }

            var filePath = Path.Combine(resources, fileName);
            Write(data, filePath);
        }

        public static T LoadFromResources(string fileName, T defaultValue = default) {
            var filePath = Path.Combine(resources, fileName);
            return Read(filePath, defaultValue);
        }
        #endregion
    }
}
