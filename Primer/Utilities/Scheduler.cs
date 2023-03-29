using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer
{
    public class Scheduler
    {
        private readonly int delay;
        private readonly Action action;
        private CancellationTokenSource cancellationSource;

        public Scheduler(Action action, int milliseconds = 10)
        {
            delay = milliseconds;
            this.action = action;
        }

        private void Cancel()
        {
            if (cancellationSource is null)
                return;

            cancellationSource.Cancel();
            cancellationSource = null;
        }

        public void Schedule()
        {
            Cancel();
            cancellationSource = new CancellationTokenSource();
            Task.Delay(delay, cancellationSource.Token).ContinueWith((_) => action());
        }
    }
}
