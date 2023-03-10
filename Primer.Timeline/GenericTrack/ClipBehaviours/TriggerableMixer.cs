using System.Collections.Generic;
using System.Linq;

namespace Primer.Timeline
{
    internal class TriggerableMixer
    {
        private readonly HashSet<TriggerablePlayable> ranTriggers = new();

        public void Mix(TriggerablePlayable[] allBehaviours, float time)
        {
            var behaviours = allBehaviours.Where(x => x.start <= time).ToArray();
            var toClean = allBehaviours.Select(x => x.triggerable).ToHashSet();
            var toExecute = new Queue<TriggerablePlayable>();
            var alreadyExecuted = new Queue<TriggerablePlayable>(ranTriggers);
            var rerunPipeline = false;

            for (var i = 0; i < behaviours.Length; i++) {
                var behaviour = behaviours[i];

                if (alreadyExecuted.Count == 0) {
                    toExecute.Enqueue(behaviour);
                    continue;
                }

                if (behaviour.Equals(alreadyExecuted.Dequeue())) {
                    toClean.Remove(behaviour.triggerable);
                    continue;
                }

                rerunPipeline = true;
                break;
            }

            // We have executions from previous frames that we need to remove, we have to re-run the whole chain
            if (rerunPipeline || alreadyExecuted.Count > 0) {

                foreach (var ran in ranTriggers)
                    ran.Cleanup();

                ranTriggers.Clear();
                toExecute = new Queue<TriggerablePlayable>(behaviours);
            }

            foreach (var behaviour in toExecute) {
                behaviour.Execute(time);
                ranTriggers.Add(behaviour);
                toClean.Remove(behaviour.triggerable);
            }

            foreach (var behaviour in toClean.SelectMany(x => allBehaviours.Where(y => y.triggerable == x))) {
                behaviour.Cleanup();
            }
        }
    }
}
