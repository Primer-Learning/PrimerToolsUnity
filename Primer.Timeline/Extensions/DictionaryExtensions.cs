using System.Collections.Generic;

namespace Primer.Timeline
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = null)
            where TValue : class
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }
    }
}
