using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Serializable]
    internal class PlayAndScrub : PlayModeBehaviour
    {
        public PlayableDirector director;
        public float to;

        protected override async void Action()
        {
            await PrimerTimeline.ScrubTo(director, to);
        }
    }
}
