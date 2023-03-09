using System;
using System.Linq;
using Primer.Timeline.FakeUnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(GenericClip))]
    public class GenericClipEditor : OdinEditor
    {
        private GenericClip clip => (GenericClip)target;

        public override void OnInspectorGUI()
        {
            clip.resolver ??= TimelineEditor.inspectedDirector;
            clip.trackTarget = GetTrackTarget();

            base.OnInspectorGUI();

            ShowCodeEditorLink();

            if (clip.expectedDuration is null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tween duration", clip.expectedDuration.ToString());

            if (clip.expectedDuration != clip.duration) {
                EditorGUILayout.HelpBox($"Clip duration doesn't match tween duration", MessageType.Warning);

                if (GUILayout.Button($"Change clip duration to {clip.expectedDuration}", GUILayout.Height(32)))
                    SetClipDuration(clip);
            }
        }

        private void ShowCodeEditorLink()
        {
            MonoBehaviour script = clip.template switch {
                ScrubbablePlayable _ => null,
                TriggerablePlayable b => b.triggerable,
                SequencePlayable c => c.sequence,
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (script != null) {
                EditorGUI.BeginDisabledGroup(true);

                // It can be a MonoBehaviour or a ScriptableObject
                var monoScript = MonoScript.FromMonoBehaviour(script);
                EditorGUILayout.ObjectField("Open editor", monoScript, typeof(MonoBehaviour), false);

                EditorGUI.EndDisabledGroup();
            }
        }

        private Transform GetTrackTarget()
        {
            var director = TimelineEditor.inspectedDirector;
            var track = director.GetTrackOfClip(clip);
            var bounds = director.GetGenericBinding(track);
            var transform = bounds as Transform;

            if (transform == null) {
                GenericTrack.LogNoTrackTargetWarning();
            }

            return transform;
        }


        #region void SetClipDuration(GenericClip asset)
        private static void SetClipDuration(GenericClip asset)
        {
            if (!asset.expectedDuration.HasValue)
                return;

            var track = TimelineEditor.inspectedDirector.GetTrackOfClip(asset);
            var clips = track.GetClips().ToArray();

            for (var i = 0; i < clips.Length; i++) {
                if (clips[i].asset != asset)
                    continue;

                IncreaseClipDuration(clips, i, asset.expectedDuration.Value);
                return;
            }
        }

        private static void IncreaseClipDuration(TimelineClip[] clips, int i, float expectedDuration)
        {
            var targetClip = clips[i];
            var neededSpace = expectedDuration - targetClip.duration;

            if (i == clips.Length - 1) {
                targetClip.duration = expectedDuration;
                return;
            }

            var nextClip = clips[i + 1];
            var availableSpace = nextClip.start - targetClip.end;

            if (availableSpace < neededSpace)
                MoveNextClips(clips, i + 1, neededSpace - availableSpace);

            targetClip.duration += neededSpace;
        }

        private static void MoveNextClips(TimelineClip[] clips, int i, double neededSpace)
        {
            if (i == clips.Length - 1) {
                clips[i].start += neededSpace;
                return;
            }

            var targetClip = clips[i];
            var nextClip = clips[i + 1];
            var availableSpace = nextClip.start - targetClip.end;

            if (neededSpace > availableSpace)
                MoveNextClips(clips, i + 1, neededSpace - availableSpace);

            targetClip.start += neededSpace;
        }
        #endregion
    }
}
