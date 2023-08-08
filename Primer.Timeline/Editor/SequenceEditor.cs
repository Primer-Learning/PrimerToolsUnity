using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Timeline.FakeUnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

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

            var clips = new Stack<TimelineClip>(track.GetClips().Where(x => x.asset is SequenceClip));
            var runner = sequence.Run();
            const float gap = 0.5f;
            var time = gap;

            while (runner.hasMoreClips) {
                var tween = await runner.NextClip();
                var clip = clips.Count is 0
                    ? track.CreateClip<SequenceClip>()
                    : clips.Pop();

                clip.start = time;
                clip.duration = tween?.duration ?? Tween.DEFAULT_DURATION;
                time = (float)clip.end + gap;

                tween?.Apply();
            }

            foreach (var clip in clips)
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
