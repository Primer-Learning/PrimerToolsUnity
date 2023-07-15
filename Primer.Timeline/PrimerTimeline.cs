using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public static class PrimerTimeline
    {
        public static bool isPlaying => PatchPlayMode.isPlaying;
        public static bool isPreloading => PatchPlayMode.isPreloading;

        static PrimerTimeline()
        {
            ContainerEvents.deactivateEventsIf = () => !isPlaying;
        }

        public static UniTask RegisterOperation(UniTask request)
        {
            return TimelineAsynchrony.RegisterOperation(request);
        }

        public static UniTask<T> RegisterOperation<T>(UniTask<T> request)
        {
            return TimelineAsynchrony.RegisterOperation(request);
        }

        public static UniTask EnterPlayMode()
        {
            if (Application.isPlaying)
                throw new InvalidOperationException("PrimerTimeline.EnterPlayMode() can only be called in Edit Mode.");

            var tcs = new UniTaskCompletionSource();
            PatchPlayMode.OnEnterPlayMode(() => tcs.TrySetResult());
            EditorApplication.EnterPlaymode();
            return tcs.Task;
        }

        public static async UniTask ScrubTo(PlayableDirector director, double time)
        {
            director.time = time < 0 ? 0 : time;
            director.Evaluate();

            await SequenceOrchestrator.AllSequencesFinished();
            await TimelineAsynchrony.AllOperationsFinished();
        }
    }
}
