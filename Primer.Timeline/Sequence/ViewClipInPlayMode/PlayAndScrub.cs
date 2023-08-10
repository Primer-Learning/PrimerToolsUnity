using System;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class PlayAndScrub : ViewInPlayModeBehaviour
    {
        [SerializeField] private float to;

        public PlayAndScrub(float to)
        {
            this.to = to;
        }

        public override async void Execute()
        {
            var director = FindDirector();

            if (director is not null)
                await PrimerTimeline.ScrubTo(director, to);
        }
    }
}
