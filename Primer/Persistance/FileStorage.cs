using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Primer
{
    public class FileStorage<T>
    {
        public static FileStorage<T> CreateForScript(string scriptPath, string extension, T defaultValue)
        {
            var dir = Path.GetDirectoryName(scriptPath);
            var name = $"{Path.GetFileNameWithoutExtension(scriptPath)}.{extension}";
            return new FileStorage<T>(dir, name, defaultValue);
        }

        public static FileStorage<T> CreateInResources(string filename, T defaultValue)
        {
            var path = Path.Combine(Application.dataPath, "Resources", filename);
            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            return new FileStorage<T>(dir, file, defaultValue);
        }

        private readonly T initialValue;
        private readonly string filename;
        private readonly string directory;

        private FileStorage(string directory, string filename, T initialValue = default)
        {
            this.initialValue = initialValue;
            this.directory = directory;
            this.filename = filename;
        }

        public T Read()
        {
            if (!File.Exists(filename))
                return initialValue;

            try {
                using var stream = File.Open(filename, FileMode.Open);
                var formatter = new BinaryFormatter();
                var deserialized = formatter.Deserialize(stream);
                return (T)deserialized;
            }
            catch (Exception ex) {
                Debug.Log($"Unable to deserialize {filename}:\n{ex}");
                return initialValue;
            }
        }

        public void Write(T data)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var stream = File.Open(filename, FileMode.Create);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }

        public void OpenFolder()
        {
            Process.Start(directory);
        }
    }
}
