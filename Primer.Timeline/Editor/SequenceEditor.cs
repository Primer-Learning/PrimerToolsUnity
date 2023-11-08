using System.Linq;
using Primer.Animation;
using Primer.Timeline.FakeUnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(Sequence), editorForChildClasses: true)]
    public class SequenceEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Evaluate"))
                Reevaluate();

            if (GUILayout.Button("Create clips"))
                CreateClips();
        }

        private async void CreateClips()
        {
            var sequence = (Sequence)target;
            var director = TimelineEditor.inspectedDirector;
            var track = director?.GetOrCreateTrack<SequenceTrack>(sequence);

            if (track == null)
                return;

            var existingClips = track.GetClips().Where(x => x.asset is SequenceClip).ToList();
            var runner = sequence.Run();
            const float gap = 0.0f;
            var time = gap;

            while (runner.hasMoreClips) {
                var tween = await runner.NextClip();

                // HACK: We don't know if we reached the end until after we call .NextClip()
                //
                // This is because I designed SequenceRunner to be like Javascript's enumerators
                //   where .Next() returns { value: T, done: boolean }
                // But C# enumerators work the other way around and I didn't see this until it was too late
                // I'm sorry 🥲
                // It can be solved if we change sequenceRunner.NextClip() return a bool instead of a Tween
                //   and add sequenceRunner.current to get the current Tween
                // But it's too big of a change to make at this point and this hack may only be needed here
                if (!runner.hasMoreClips)
                    break;

                var clip = existingClips.FirstOrDefault(
                    x => string.IsNullOrEmpty(tween?.name)
                        ? x.IsNameAutomated()
                        : x.displayName == tween.name
                );

                if (clip is null)
                    clip = track.CreateClip<SequenceClip>();
                else
                    existingClips.Remove(clip);

                if (!clip.IsLocked()) {
                    clip.start = time;
                    clip.duration = tween?.duration ?? Tween.DEFAULT_DURATION;
                    clip.displayName = tween?.name ?? "";

                    if (clip.asset is PrimerClip primerClip)
                        primerClip.clipColor = sequence.clipColor;
                }

                time = (float)clip.end + gap;
                tween?.Apply();
            }

            foreach (var clip in existingClips)
                track.DeleteClip(clip);

            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

        private async void Reevaluate()
        {
            var director = TimelineEditor.inspectedDirector;
            if (director == null) return;

            var player = SequenceOrchestrator.GetPlayerFor((Sequence)target);
            await player.Reset();
            director.Evaluate();
        }
    }
}
