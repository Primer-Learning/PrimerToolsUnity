using System;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [ExecuteAlways]
    public class ShowForXSeconds : TriggeredAnimation
    {
        public float seconds = 1;
        private PrimerBehaviour primer;

        public void Prepare()
        {
            this.Log("Reset state");
            primer = this.GetPrimer();

            // This saves the current position and re-sets it
            primer.FindIntrinsicPosition();
            primer.ApplyIntrinsicPosition();

            transform.localScale = Vector3.zero;
        }

        public async void MyCustomNamedTrigger()
        {
            this.Log("ScaleUpFromZero");
            await primer.ScaleUpFromZero();
            this.Log($"Now wait {seconds} seconds");
            await Wait(Mathf.RoundToInt(seconds * 1000));
            this.Log("MyCustomNamedTrigger done");
        }

        public async void MoveUp()
        {
            var pos = transform.position;

            this.Log("Move UP!");
            await primer.MoveTo(new Vector3(pos.x, pos.y + 1, pos.z));
            this.Log("Movement complete");
        }

        public async void AnotherTrigger()
        {
            this.Log("I'm going to wait");
            await Wait(1000);
            this.Log("Thanks for your patience");
        }
    }
}
