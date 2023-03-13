using System;
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
        private GenericClip clipAsset => (GenericClip)target;

        public override void OnInspectorGUI()
        {
            clipAsset.resolver ??= TimelineEditor.inspectedDirector;
            clipAsset.trackTarget = GetTrackTarget();

            base.OnInspectorGUI();

            ShowCodeEditorLink();

            if (clipAsset.expectedDuration is null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tween duration", clipAsset.expectedDuration.ToString());

            var clipDuration = (float)FindClipFor(clipAsset).duration;

            if (clipAsset.expectedDuration != clipDuration) {
                EditorGUILayout.HelpBox($"Clip duration doesn't match tween duration", MessageType.Warning);

                if (GUILayout.Button($"Change clip duration to {clipAsset.expectedDuration}", GUILayout.Height(32)))
                    clipAsset.SetClipDuration(clipAsset.expectedDuration.Value);
            }
        }

        private void ShowCodeEditorLink()
        {
            MonoBehaviour script = clipAsset.template switch {
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
            var track = director.GetTrackOfClip(clipAsset);
            var bounds = director.GetGenericBinding(track);
            var transform = bounds as Transform;

            if (transform == null) {
                GenericTrack.LogNoTrackTargetWarning();
            }

            return transform;
        }


        private static TimelineClip FindClipFor(GenericClip asset)
        {
            var clips = TimelineEditor.inspectedDirector.GetClipsInTrack(asset);
            var index = Array.FindIndex(clips, c => c.asset == asset);
            return clips[index];
        }
    }
}
