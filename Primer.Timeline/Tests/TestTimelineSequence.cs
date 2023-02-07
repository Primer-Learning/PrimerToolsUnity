using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline.Tests
{
    public class TestTimelineSequence : Sequence
    {
        public float seconds = 1;
        private PrimerBehaviour primer;

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

        public override async IAsyncEnumerator<object> Run()
        {
            this.Log("ScaleUpFromZero");
            await primer.ScaleUpFromZero();
            this.Log($"Now wait {seconds} seconds");
            await Seconds(seconds);
            this.Log("MyCustomNamedTrigger done");

            yield return null;

            var pos = transform.position;

            this.Log("Move UP!");
            await primer.MoveTo(new Vector3(pos.x, pos.y + 1, pos.z));
            this.Log("Movement complete");

            yield return null;

            this.Log("I'm going to wait");
            await Milliseconds(500);
            this.Log("Thanks for your patience");
        }
    }
}
