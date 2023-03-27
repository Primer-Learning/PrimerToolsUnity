using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Primer
{
    public class WeakSet<T> : IEnumerable<T> where T : class
    {
        private readonly HashSet<WeakReference<T>> set = new();

        public void Add(T item)
        {
            if (Contains(item))
                return;
            
            set.Add(new WeakReference<T>(item));
        }

        public void Remove(T item)
        {
            set.RemoveWhere(x => x.TryGetTarget(out var t) && t == item);
        }

        public bool Contains(T item)
        {
            return set.Any(x => x.TryGetTarget(out var t) && t == item);
        }

        public void Purge()
        {
            set.RemoveWhere(x => !x.TryGetTarget(out _));
        }

        public IEnumerator<T> GetEnumerator()
        {
            Purge();

            var copy = new HashSet<T>(set.Select(x => x.TryGetTarget(out var t) ? t : null));
            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
