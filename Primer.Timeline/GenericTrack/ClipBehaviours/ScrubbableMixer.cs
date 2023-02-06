using System.Linq;

namespace Primer.Timeline
{
    internal class ScrubbableMixer
    {
        public void Mix(ScrubbablePlayable[] behaviours, float time, uint iteration)
        {
            var unused = behaviours.Select(x => x.scrubbable.GetType()).ToHashSet();

            for (var i = 0; i < behaviours.Length; i++) {
                var behaviour = behaviours[i];

                if (behaviour.weight != 0) {
                    behaviour.Execute(time);
                    unused.Remove(behaviour.scrubbable.GetType());
                }
                else if (behaviour.end < time) {
                    behaviour.Execute(1);
                    unused.Remove(behaviour.scrubbable.GetType());
                }
            }

            foreach (var scrubbable in unused)
                ScrubbablePlayable.Cleanup(behaviours.First(x => x.scrubbable.GetType() == scrubbable).scrubbable);
        }
    }
}
