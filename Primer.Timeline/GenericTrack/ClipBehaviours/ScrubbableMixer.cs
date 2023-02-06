namespace Primer.Timeline
{
    internal class ScrubbableMixer
    {
        public void Mix(ScrubbablePlayable[] behaviours, float time, uint iteration)
        {
            for (var i = 0; i < behaviours.Length; i++) {
                var behaviour = behaviours[i];

                if (behaviour.weight != 0)
                    behaviour.Execute(time);
                else if (behaviour.end < time)
                    behaviour.Execute(1);
                else
                    behaviour.Cleanup();
            }
        }
    }
}
