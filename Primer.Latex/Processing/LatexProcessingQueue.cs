using System;
using System.Threading;
using System.Threading.Tasks;

namespace Primer.Latex
{
    public class LatexProcessingQueue : ILatexProcessor
    {
        private readonly ILatexProcessor processor;
        private LatexInput queue;
        private TaskCompletionSource<LatexChar[]> queuedTask;

        public LatexProcessingState state => processor.state;


        public LatexProcessingQueue(ILatexProcessor innerProcessor)
        {
            processor = innerProcessor;
        }


        public Task<LatexChar[]> Render(LatexInput config, CancellationToken cancellationToken = default)
        {
            return processor.state == LatexProcessingState.Processing
                ? SetQueue(config)
                : Process(config, cancellationToken);

        }

        private async Task<LatexChar[]> Process(LatexInput config, CancellationToken cancellationToken)
        {
            var result = await processor.Render(config, cancellationToken);
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
                completionSource.SetResult(await Render(config));
            }
            catch (Exception err) {
                completionSource.SetException(err);
            }
        }

#if UNITY_EDITOR
        public void OpenBuildDir() => processor.OpenBuildDir();
#endif
    }
}
