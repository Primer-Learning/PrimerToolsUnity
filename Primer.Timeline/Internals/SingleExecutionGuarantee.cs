using System.Threading;

namespace Primer.Timeline
{
    public class SingleExecutionGuarantee
    {
        private CancellationTokenSource lastExecution = new();

        public CancellationToken token => lastExecution.Token;

        public CancellationToken NewExecution()
        {
            if (lastExecution is not null)
                lastExecution.Cancel();

            lastExecution = new CancellationTokenSource();
            return lastExecution.Token;
        }
    }
}
