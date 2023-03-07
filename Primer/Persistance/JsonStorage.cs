using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Primer
{
    public class JsonStorage<T> : FileStorage<T>
    {
        public static JsonStorage<T> CreateForScript(string scriptPath, T defaultValue)
        {
            var dir = Path.GetDirectoryName(scriptPath);
            var name = $"{Path.GetFileNameWithoutExtension(scriptPath)}.json";
            return new JsonStorage<T>(dir, name, defaultValue);
        }

        public static JsonStorage<T> CreateInResources(string filename, T defaultValue)
        {
            var path = Path.Combine(Application.dataPath, "Resources", filename);
            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            return new JsonStorage<T>(dir, file, defaultValue);
        }

        public JsonStorage(string directory, string filename, T initialValue = default)
            : base(directory, filename, initialValue) {}


        protected override void Serialize(FileStream stream, T data)
        {
            var json = data is IEnumerable<object> list
                ? SerializeList(list)
                : JsonUtility.ToJson(data, true);

            using var writer = new StreamWriter(stream);
            writer.Write(json);
        }

        private static string SerializeList(IEnumerable<object> list)
        {
            var json = list.Select(JsonUtility.ToJson);
            return "[\n" + string.Join(",\n", json) + "\n]";
        }

        protected override T Deserialize(FileStream stream)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            return json.StartsWith("[")
                ? JsonUtility.FromJson<JsonList<T>>("{\"__data__\":" + json + "}").__data__
                : JsonUtility.FromJson<T>(json);
        }

        public JsonStorage<T> BackupFile(uint index = 0)
        {
            return new JsonStorage<T>(directory, $"{filename}.backup{index}", initialValue);
        }

        [UsedImplicitly]
        private class JsonList<TList>
        {
            // ReSharper disable once InconsistentNaming
            public TList __data__;
        }
    }
}
