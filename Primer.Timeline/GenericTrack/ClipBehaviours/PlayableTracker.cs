using System;
using System.Collections.Generic;

namespace Primer.Timeline
{
    public class PlayableTracker<T> where T : class
    {
        private const bool CLEAN = true;
        private const bool PREPARED = false;

        private static Dictionary<WeakReference<T>, bool> tracker = new();

        public bool IsClean(T playable)
        {
            if (playable == null)
                return true;

            var key = new WeakReference<T>(playable);

            if (!tracker.ContainsKey(key)) {
                tracker.Add(key, CLEAN);
                return false;
            }

            var isClean = tracker[key] == CLEAN;

            if (!isClean)
                tracker[key] = CLEAN;

            return isClean;
        }

        public bool IsPrepared(T playable)
        {
            if (playable == null)
                return true;

            var key = new WeakReference<T>(playable);

            if (!tracker.ContainsKey(key)) {
                tracker.Add(key, PREPARED);
                return false;
            }

            var isPrepared = tracker[key] == PREPARED;

            if (!isPrepared)
                tracker[key] = PREPARED;

            return isPrepared;
        }
    }
}
