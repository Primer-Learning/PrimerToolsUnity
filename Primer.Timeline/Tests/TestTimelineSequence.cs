using System.Collections.Generic;
using Primer.Animation;
using Vector3 = UnityEngine.Vector3;

namespace Primer.Timeline.Tests
{
    public class TestTimelineSequence : Sequence
    {
        public float seconds = 1;
        private PrimerComponent primer;

        public override void Cleanup()
        {
            this.Log("Reset state");
            primer = this.GetPrimer();

            primer.FindIntrinsicScale();
            transform.localScale = Vector3.zero;

            // This saves the current position and re-sets it
            primer.FindIntrinsicPosition();
            primer.ApplyIntrinsicPosition();
        }

        public override async IAsyncEnumerator<Tween> Define()
        {
            this.Log("ScaleUpFromZero");
            yield return primer.ScaleUpFromZero();

            this.Log($"Now wait {seconds} seconds");
            await Seconds(seconds);
            this.Log("MyCustomNamedTrigger done");
            yield return null;

            this.Log("Move UP!");
            yield return primer.MoveTo(transform.position + Vector3.up);

            this.Log("I'm going to wait");
            await Milliseconds(500);
            this.Log("Thanks for your patience");
        }
    }
}
