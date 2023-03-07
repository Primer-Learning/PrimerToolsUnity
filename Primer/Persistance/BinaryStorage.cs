using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Primer
{
    public class BinaryStorage<T> : FileStorage<T>
    {
        public static BinaryStorage<T> CreateForScript(string scriptPath, T defaultValue)
        {
            var dir = Path.GetDirectoryName(scriptPath);
            var name = $"{Path.GetFileNameWithoutExtension(scriptPath)}.bin";
            return new BinaryStorage<T>(dir, name, defaultValue);
        }

        public static BinaryStorage<T> CreateInResources(string filename, T defaultValue)
        {
            var path = Path.Combine(Application.dataPath, "Resources", filename);
            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            return new BinaryStorage<T>(dir, file, defaultValue);
        }


        public BinaryStorage(string directory, string filename, T initialValue = default)
            : base(directory, filename, initialValue) {}


        protected override void Serialize(FileStream stream, T data)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, data);
        }

        protected override T Deserialize(FileStream stream)
        {
            var formatter = new BinaryFormatter();
            var deserialized = formatter.Deserialize(stream);
            return (T)deserialized;
        }
    }
}
