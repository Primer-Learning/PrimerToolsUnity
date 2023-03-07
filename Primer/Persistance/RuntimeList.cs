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
        public List<T> items = new();
        private JsonStorage<List<T>> file;
        // private BinaryStorage<List<T>> fileIn;

        /// <summary>
        /// Use this method only to call .CreateStorage(), C# will do the rest
        /// </summary>
        protected abstract void Initialize();

        protected void CreateStorage([CallerFilePath] string scriptPath = null)
        {
            file ??= JsonStorage<List<T>>.CreateForScript(scriptPath, new List<T>());
            // fileIn ??= BinaryStorage<List<T>>.CreateForScript(scriptPath, new List<T>());
        }

        protected virtual void OnEnable() => ReadFromDisk();
        protected virtual void OnDisable() => WriteToDisk();

        protected void ReadFromDisk()
        {
            if (file is null)
                Initialize();

            items = file!.Read();
        }

        protected void WriteToDisk()
        {
            if (file is null)
                Initialize();

            if (items.Count > 0)
                file!.Write(items);
        }


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
