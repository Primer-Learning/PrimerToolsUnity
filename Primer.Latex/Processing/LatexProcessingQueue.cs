using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    internal class LatexProcessingQueue : ProcessingPipeline
    {
        private LatexInput queue;
        private TaskCompletionSource<LatexExpression> queuedTask;

        public LatexProcessingQueue(ILatexProcessor innerProcessor) : base(innerProcessor) {}


        public override Task<LatexExpression> Process(LatexInput config, CancellationToken cancellationToken = default)
            => processor.state == LatexProcessingState.Processing
                ? SetQueue(config)
                : DelegateProcessing(config, cancellationToken);

        private async Task<LatexExpression> DelegateProcessing(LatexInput config, CancellationToken cancellationToken)
        {
            var result = await processor.Process(config, cancellationToken);
            ProcessQueue();
            return result;
        }

        private Task<LatexExpression> SetQueue(LatexInput config)
        {
            if (queue is not null) {
                if (queue.Equals(config))
                    return queuedTask.Task;

                queuedTask.SetException(new OperationCanceledException("New LaTeX evaluation requested, clearing queue"));
            }


            queue = config;
            queuedTask = new TaskCompletionSource<LatexExpression>();
            return queuedTask.Task;
        }

        private async void ProcessQueue()
        {
            if (queue is null)
                return;

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
