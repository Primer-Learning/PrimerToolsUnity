using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Primer.Timeline
{
    internal class PlayAndFastForward : PlayModeBehaviour
    {
        public PlayableDirector director;
        [FormerlySerializedAs("at")] public float to;
        public float frameRate;

        protected override async void Action()
        {
            var fps = frameRate > 0 ? frameRate : Time.captureFramerate;

            if (fps <= 0)
                throw new Exception("Can't fast forward because frameRate is 0 and Time.captureFramerate is 0.");

            var secondsPerFrame = 1f / fps;

            director.Pause();
            PatchPlayMode.isPlaying = false;

            while (director.time < to) {
                await PrimerTimeline.ScrubTo(director, director.time + secondsPerFrame);
            }

            // cautionary wait to let the director catch up
            await UniTask.Delay(100);

            PatchPlayMode.isPlaying = true;
            director.Play();        }
    }
}
