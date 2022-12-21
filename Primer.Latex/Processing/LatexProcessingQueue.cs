using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessingQueue : ProcessingPipeline
    {
        private LatexInput queue;
        private TaskCompletionSource<LatexChar[]> queuedTask;

        public LatexProcessingQueue(ILatexProcessor innerProcessor) : base(innerProcessor) {}


        public override Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken = default)
        {
            return processor.state == LatexProcessingState.Processing
                ? SetQueue(config)
                : DelegateProcessing(config, cancellationToken);

        }

        private async Task<LatexChar[]> DelegateProcessing(LatexInput config, CancellationToken cancellationToken)
        {
            var result = await processor.Process(config, cancellationToken);
            ProcessQueue();
            return result;
        }

        private Task<LatexChar[]> SetQueue(LatexInput config)
        {
            if (queue is not null) {
                if (queue.Equals(config))
                    return queuedTask.Task;

                queuedTask.SetException(new OperationCanceledException("Scheduled execution removed"));
            }


            queue = config;
            queuedTask = new TaskCompletionSource<LatexChar[]>();
            return queuedTask.Task;
        }

        private async void ProcessQueue()
        {
            if (queue is null) return;

            // Release thread and continue after
            await Task.Yield();

            var config = queue;
            var completionSource = queuedTask;

            queue = null;
            queuedTask = null;

            try {
                completionSource.SetResult(await Process(config));
            }
            catch (Exception err) {
                completionSource.SetException(err);
            }
        }
    }
}
