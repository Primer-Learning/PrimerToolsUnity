using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace Primer
{
    public abstract class FileStorage<T>
    {
        protected readonly T initialValue;
        protected readonly string filename;
        protected readonly string directory;

        public string path => Path.Combine(directory, filename);

        protected FileStorage(string directory, string filename, T initialValue = default)
        {
            this.initialValue = initialValue;
            this.directory = directory;
            this.filename = filename;
        }

        protected abstract void Serialize(FileStream stream, T data);
        protected abstract T Deserialize(FileStream stream);

        public bool Exists()
        {
            return File.Exists(path);
        }

        public T Read()
        {
            if (!Exists())
                return initialValue;

            // PrimerLogger.Log("Read", path);

            try {
                using var stream = File.Open(path, FileMode.Open);
                return Deserialize(stream);
            }
            catch (Exception ex) {
                Debug.Log($"Unable to deserialize {path}:\n{ex}");
                return initialValue;
            }
        }

        public void Write(T data)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // PrimerLogger.Log("Write", path);

            using var stream = File.Open(path, FileMode.Create);
            Serialize(stream, data);
        }

        public void OpenFolder()
        {
            Process.Start(directory);
        }
    }
}
