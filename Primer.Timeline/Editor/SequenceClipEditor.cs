using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [CustomEditor(typeof(SequenceClip))]
    public class SequenceClipEditor : OdinEditor
    {
        private SequenceClip clipAsset => (SequenceClip)target;

        public override void OnInspectorGUI()
        {
            clipAsset.resolver ??= TimelineEditor.inspectedDirector;
            base.OnInspectorGUI();
            CodeEditorLink();
            ExpectedDuration();
        }

        private void CodeEditorLink()
        {
            EditorGUI.BeginDisabledGroup(true);

            // It can be a MonoBehaviour or a ScriptableObject
            var monoScript = MonoScript.FromMonoBehaviour(GetTrackTarget());
            EditorGUILayout.ObjectField("Sequence", monoScript, typeof(MonoBehaviour), false);

            EditorGUI.EndDisabledGroup();
        }

        private void ExpectedDuration()
        {
            if (clipAsset.expectedDuration is null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tween duration", clipAsset.expectedDuration.ToString());

            var clipDuration = (float)FindClipFor(clipAsset).duration;

            if (clipAsset.expectedDuration == clipDuration)
                return;

            EditorGUILayout.HelpBox($"Clip duration doesn't match tween duration", MessageType.Warning);

            if (GUILayout.Button($"Change clip duration to {clipAsset.expectedDuration}", GUILayout.Height(32))) {
                clipAsset.SetClipDuration(clipAsset.expectedDuration.Value);
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        private Sequence GetTrackTarget()
        {
            var director = TimelineEditor.inspectedDirector;
            var track = director.GetTrackOfClip(clipAsset);
            var bounds = director.GetGenericBinding(track);
            var sequence = bounds as Sequence;

            if (sequence == null) {
                PrimerTrack.LogNoTrackTargetWarning();
            }

            return sequence;
        }

        private static TimelineClip FindClipFor(SequenceClip asset)
        {
            var clips = TimelineEditor.inspectedDirector.GetClipsInTrack(asset);
            var index = Array.FindIndex(clips, c => c.asset == asset);
            return clips[index];
        }
    }
}
