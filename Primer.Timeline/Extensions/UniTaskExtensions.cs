using System;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    public static class UniTaskExtensions
    {
        public static UniTaskCompletionSource<T> ToSource<T>(this UniTask<T> task)
        {
            var tcs = new UniTaskCompletionSource<T>();
            WaitForResult(tcs, task);
            return tcs;
        }

        public static UniTaskCompletionSource ToSource(this UniTask task)
        {
            var tcs = new UniTaskCompletionSource();
            WaitForResult(tcs, task);
            return tcs;
        }

        private static async void WaitForResult<T>(IPromise<T> source, UniTask<T> task)
        {
            T result;

            try {
                result = await task;
            } catch (Exception e) {
                source.TrySetException(e);
                return;
            }

            source.TrySetResult(result);
        }

        private static async void WaitForResult(IPromise source, UniTask task)
        {
            try {
                await task;
            } catch (Exception e) {
                source.TrySetException(e);
                return;
            }

            source.TrySetResult();
        }
    }
}
