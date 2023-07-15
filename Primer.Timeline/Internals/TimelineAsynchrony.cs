using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal static class TimelineAsynchrony
    {
        private static readonly List<Func<UniTask>> operations = new();

        public static UniTask<T> RegisterOperation<T>(UniTask<T> request)
        {
            var source = request.ToSource();

            // We create a function that returns a new task each time
            UniTask Operation() => source.Task;

            // and add it to the list if things that need to finish
            // before we can consider the timeline scrubbed
            operations.Add(Operation);

            // the function should be removed when the task done
            RemoveWhenCompleted(Operation);

            return source.Task;
        }

        public static UniTask RegisterOperation(UniTask request)
        {
            var source = request.ToSource();

            // We create a function that returns a new task each time
            UniTask Operation() => source.Task;

            // and add it to the list if things that need to finish
            // before we can consider the timeline scrubbed
            operations.Add(Operation);

            // the function should be removed when the task done
            RemoveWhenCompleted(Operation);

            return source.Task;
        }

        private static async void RemoveWhenCompleted(Func<UniTask> operation)
        {
            try {
                await operation();
            }
            finally {
                operations.Remove(operation);
            }
        }

        public static async UniTask AllOperationsFinished()
        {
            await UniTask.WhenAll(operations.Select(x => x()));
            operations.Clear();
        }
    }}
