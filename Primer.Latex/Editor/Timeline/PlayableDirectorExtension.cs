using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEditor.LatexRenderer.Timeline
{
    public static class PlayableDirectorExtension
    {
        public static IEnumerator PlayAll(this PlayableDirector playableDirector)
        {
            playableDirector.Play();
            yield return new WaitUntil(() => playableDirector.state != PlayState.Playing ||
                                             playableDirector.time >= playableDirector.duration);
        }
    }
}