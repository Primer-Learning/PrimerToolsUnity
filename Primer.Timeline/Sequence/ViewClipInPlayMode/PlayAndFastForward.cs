using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class PlayAndFastForward : ViewInPlayModeBehaviour
    {
        [SerializeField] private float at;
        [SerializeField] private float frameRate;

        public PlayAndFastForward(float at, float frameRate)
        {
            this.at = at;
            this.frameRate = frameRate;
        }

        public override async void Execute()
        {
            var fps = frameRate > 0 ? frameRate : Time.captureFramerate;

            if (fps <= 0)
                throw new Exception("Can't fast forward because frameRate is 0 and Time.captureFramerate is 0.");

            var director = FindDirector();

            if (director is null)
                return;

            var secondsPerFrame = 1f / fps;

            director.Pause();
            PatchPlayMode.isPlaying = false;

            while (director.time < at) {
                await PrimerTimeline.ScrubTo(director, director.time + secondsPerFrame);
            }

            // cautionary wait to let the director catch up
            await UniTask.Delay(100);

            PatchPlayMode.isPlaying = true;
            director.Play();        }
    }
}
