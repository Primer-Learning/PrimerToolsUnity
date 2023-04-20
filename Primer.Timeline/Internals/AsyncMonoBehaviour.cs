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

        public void AddToParallel(Tween tween)
        {
            parallelQueue.Add(tween);
        }

        public Tween WaitForParallels()
        {
            var result = Parallel(parallelQueue.ToArray());
            parallelQueue.Clear();
            return result;
        }

        protected static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        protected static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        protected static async UniTask Parallel(params UniTask[] processes)
        {
            await UniTask.WhenAll(processes);
        }

        public static Tween Parallel(IEnumerable<Tween> tweenList) =>Parallel(tweenList.ToArray());
        public static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Parallel(tweenList);
        }

        public static Tween Series(IEnumerable<Tween> tweenList) =>Series(tweenList.ToArray());
        public static Tween Series(params Tween[] tweenList)
        {
            return Tween.Series(tweenList);
        }
    }

    public class AsyncMonoBehaviour : MonoBehaviour
    {
        private readonly List<Tween> parallelQueue = new();

        public void AddToParallel(Tween tween)
        {
            parallelQueue.Add(tween);
        }

        public Tween WaitForParallels(float delayBetweenStarts = 0)
        {
            var result = Parallel(delayBetweenStarts, parallelQueue.ToArray());
            parallelQueue.Clear();
            return result;
        }

        protected static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        protected static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        public static async UniTask Parallel(params UniTask[] processes)
        {
            await UniTask.WhenAll(processes);
        }

        public static async UniTask Series(params Func<UniTask>[] processes)
        {
            foreach (var process in processes)
            {
                await process();
            }
        }

        public static Tween Parallel(IEnumerable<Tween> tweenList) =>Parallel(0, tweenList.ToArray());
        public static Tween Parallel(params Tween[] tweenList)
        {
            return Tween.Parallel(tweenList);
        }
        public static Tween Parallel(float delayBetweenStarts = 0, params Tween[] tweenList)
        {
            return Tween.Parallel(delayBetweenStarts, tweenList);
        }

        public static Tween Series(IEnumerable<Tween> tweenList) =>Series(tweenList.ToArray());
        public static Tween Series(params Tween[] tweenList)
        {
            return Tween.Series(tweenList);
        }
    }
}
