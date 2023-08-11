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

        public static event Action onEnterPlayMode {
            add => PatchPlayMode.whenReady += value;
            remove => PatchPlayMode.whenReady -= value;
        }

        static PrimerTimeline()
        {
            GnomeEvents.deactivateEventsIf = () => !isPlaying;
        }

        public static UniTask RegisterOperation(UniTask request)
        {
            return TimelineAsynchrony.RegisterOperation(request);
        }

        public static UniTask<T> RegisterOperation<T>(UniTask<T> request)
        {
            return TimelineAsynchrony.RegisterOperation(request);
        }

        public static void EnterPlayMode()
        {
            if (Application.isPlaying)
                throw new InvalidOperationException("PrimerTimeline.EnterPlayMode() can only be called in Edit Mode.");

            // This doesn't work because Unity re-compiles the scripts when we call EditorApplication.EnterPlaymode()
            //   so the callback never gets called and the returned Task never completes.
            //
            // var tcs = new UniTaskCompletionSource();
            // PatchPlayMode.OnEnterPlayMode(() => tcs.TrySetResult());
            EditorApplication.EnterPlaymode();
            // return tcs.Task;
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
