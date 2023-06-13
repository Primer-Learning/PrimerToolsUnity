using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    // These two classes are identical, except one extends MonoBehaviour and the other doesn't

    public class Async
    {
        private readonly List<Tween> parallelQueue = new();

        public void AddToParallel(Tween tween) => parallelQueue.Add(tween);

        public Tween WaitForParallels(float delayBetweenStarts = 0)
        {
            if (parallelQueue.Count == 0)
                return Tween.noop;

            var result = Parallel(delayBetweenStarts, parallelQueue.ToArray());
            parallelQueue.Clear();
            return result;        }

        public static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        public static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        public static Tween Parallel(float delayBetweenStarts, IEnumerable<Tween> tweenList)
        {
            return Tween.Parallel(delayBetweenStarts, tweenList.ToArray());
        }

        public static Tween Parallel(float delayBetweenStarts = 0, params Tween[] tweenList)
        {
            return Tween.Parallel(delayBetweenStarts, tweenList);
        }

        public static Tween Parallel(IEnumerable<Tween> tweenList) => Parallel(tweenList.ToArray());
        public static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Parallel(tweenList);
        }

        public static Tween Series(IEnumerable<Tween> tweenList) => Series(tweenList.ToArray());
        public static Tween Series(params Tween[] tweenList)
        {
            return Tween.Series(tweenList);
        }

        public static Tween PlayModeOnly(Action playModeAction, Func<Tween> fallback = null)
        {
            return Application.isPlaying && PrimerTimeline.isPlaying
                ? Tween.noop.Observe(beforePlay: playModeAction)
                : fallback?.Invoke() ?? Tween.noop;
        }
    }

    public class AsyncMonoBehaviour : MonoBehaviour
    {
        private readonly Async internalAsync = new();

        protected void AddToParallel(Tween tween) => internalAsync.AddToParallel(tween);

        protected Tween WaitForParallels(float delayBetweenStarts = 0)
        {
            return internalAsync.WaitForParallels(delayBetweenStarts);
        }

        protected static UniTask Milliseconds(int milliseconds)
        {
            return Async.Milliseconds(milliseconds);
        }

        protected static UniTask Seconds(float seconds)
        {
            return Async.Seconds(seconds);
        }

        protected static Tween Parallel(IEnumerable<Tween> tweenList)
        {
            return Async.Parallel(tweenList);
        }

        protected static Tween Parallel(params Tween[] tweenList)
        {
            return Async.Parallel(tweenList);
        }

        protected static Tween Parallel(float delayBetweenStarts, IEnumerable<Tween> tweenList)
        {
            return Async.Parallel(delayBetweenStarts, tweenList);
        }

        protected static Tween Parallel(float delayBetweenStarts = 0, params Tween[] tweenList)
        {
            return Async.Parallel(delayBetweenStarts, tweenList);
        }

        protected static Tween Series(IEnumerable<Tween> tweenList)
        {
            return Async.Series(tweenList);
        }

        protected static Tween Series(params Tween[] tweenList)
        {
            return Async.Series(tweenList);
        }

        protected static Tween PlayModeOnly(Action playModeAction, Func<Tween> fallback = null)
        {
            return Async.PlayModeOnly(playModeAction, fallback);
        }
    }
}
