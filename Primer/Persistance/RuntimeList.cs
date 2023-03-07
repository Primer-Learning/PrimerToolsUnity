using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Primer
{
    [Serializable]
    public abstract class RuntimeList<T> : ScriptableObject, IList<T>
    {
        [HideInInspector]
        public List<T> items = new();
        private JsonStorage<List<T>> file;

        /// <summary>
        /// Use this method only to call .CreateStorage(), C# will do the rest
        /// </summary>
        protected abstract void Initialize();

        protected void CreateStorage([CallerFilePath] string scriptPath = null)
        {
            file ??= JsonStorage<List<T>>.CreateForScript(scriptPath, new List<T>());
        }

        protected virtual void OnEnable() => ReadFromDisk();
        protected virtual void OnDisable() => WriteToDisk();


        #region I/O
        private void EnsureInitialized()
        {
            if (file is null)
                Initialize();
        }

        protected void ReadFromDisk()
        {
            EnsureInitialized();
            items = file.Read();
        }

        protected void WriteToDisk()
        {
            EnsureInitialized();
            file.Write(items);
        }

        public void Backup(uint index = 0)
        {
            EnsureInitialized();
            file.BackupFile(index).Write(items);
        }

        public void RestoreBackup(uint index = 0)
        {
            EnsureInitialized();
            var backup = file.BackupFile(index);

            if (!backup.Exists()) {
                Debug.LogError($"File {backup.path} does not exist, cannot restore backup");
                return;
            }

            items = backup.Read();
        }

        public void OpenDirectory()
        {
            file.OpenFolder();
        }
        #endregion


        #region IList implementation
        public int Count => items.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        public void Add(T item) => items.Add(item);
        public bool Remove(T item) => items.Remove(item);
        public void Clear() => items.Clear();
        public bool Contains(T item) => items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => items.IndexOf(item);
        public void Insert(int index, T item) => items.Insert(index, item);
        public void RemoveAt(int index) => items.RemoveAt(index);

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
