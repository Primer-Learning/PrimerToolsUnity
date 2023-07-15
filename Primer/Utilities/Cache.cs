using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Primer
{
    public class Cache<TKey, TValue>
    {
        [Serializable]
        private class Entry
        {
            public int hash;
            public TValue data;
        }

        private readonly JsonStorage<Entry[]> file;
        private readonly Dictionary<int, TValue> cache = new();

        public Cache([CallerFilePath] string scriptPath = null)
        {
            file = JsonStorage<Entry[]>.CreateForScript(scriptPath, Array.Empty<Entry>());

            var stored = file.Read();
            if (stored is null) return;

            foreach (var keyValue in stored)
                cache[keyValue.hash] = keyValue.data;
        }

        public bool Peek(TKey key, out TValue value)
        {
            var hash = key.GetHashCode();
            return cache.TryGetValue(hash, out value);
        }

        public void Save(TKey key, TValue value)
        {
            var hash = key.GetHashCode();
            cache[hash] = value;

            var serializable = cache.Select(x => new Entry { hash = x.Key, data = x.Value });
            file.Write(serializable.ToArray());
        }
    }

    public class Cache<TKey1, TKey2, TValue> : Cache<(TKey1, TKey2), TValue>
    {
        public Cache([CallerFilePath] string scriptPath = null) : base(scriptPath) { }
        public bool Peek(TKey1 key1, TKey2 key2, out TValue value) => Peek((key1, key2), out value);
        public void Save(TKey1 key1, TKey2 key2, TValue value) => Save((key1, key2), value);
    }

    public class Cache<TKey1, TKey2, TKey3, TValue> : Cache<(TKey1, TKey2, TKey3), TValue>
    {
        public Cache([CallerFilePath] string scriptPath = null) : base(scriptPath) { }
        public bool Peek(TKey1 key1, TKey2 key2, TKey3 key3, out TValue value) => Peek((key1, key2, key3), out value);
        public void Save(TKey1 key1, TKey2 key2, TKey3 key3, TValue value) => Save((key1, key2, key3), value);
    }
}
