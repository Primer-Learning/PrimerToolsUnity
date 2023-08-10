using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class PlayAndPauseBefore : ViewInPlayModeBehaviour
    {
        [SerializeField] private float at;

        public PlayAndPauseBefore(float at)
        {
            this.at = at;
        }

        public override async void Execute()
        {
            var director = FindDirector();

            if (director is null)
                return;

            PatchPlayMode.isPlaying = false;

            await UniTask.WaitWhile(() => director.time <= at);

            EditorApplication.isPaused = true;
            PatchPlayMode.isPlaying = true;
        }
    }
}
