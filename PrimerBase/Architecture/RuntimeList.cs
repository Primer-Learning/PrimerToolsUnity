using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    [Serializable]
    public class RuntimeList<T> : ScriptableObject, IList<T>
    {
        public List<T> items = new();

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
    }
}
