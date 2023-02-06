using System.Collections.Generic;
using System.Linq;

namespace Primer.Timeline
{
    internal class ScrubbableMixer
    {
        private readonly Queue<ScrubbablePlayable> executed = new();

        public void Mix(ScrubbablePlayable[] allBehaviours, float time, uint iteration)
        {
            var validBehaviours = allBehaviours.Where(x => x.scrubbable is not null).ToArray();
            var toCleanup = validBehaviours.Where(x => x.start > time).ToArray();
            var toExecute = validBehaviours.Where(x => x.start <= time).ToArray();

            // We clean up behaviours after the time cursor from the end to the beginning
            foreach (var behaviour in toCleanup.Reverse())
                behaviour.Cleanup();

            foreach (var behaviour in toExecute)
                behaviour.Execute(time);
        }
    }
}
