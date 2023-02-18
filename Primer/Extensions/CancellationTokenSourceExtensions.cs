using System;
using System.Threading;

namespace Primer
{
    public static class CancellationTokenSourceExtensions
    {
        public static void TryCancel(this CancellationTokenSource tokenSource)
        {
            try {
                tokenSource.Cancel();
            }
            catch (ObjectDisposedException) {
                // ignore
            }
        }
    }
}
