using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Serializable]
    internal class PlayAndPauseBefore : PlayModeBehaviour
    {
        public PlayableDirector director;
        public float at;

        protected override async void Action()
        {
            PatchPlayMode.isPlaying = false;

            await UniTask.WaitWhile(() => director.time <= at);

            EditorApplication.isPaused = true;
            PatchPlayMode.isPlaying = true;
        }
    }
}
