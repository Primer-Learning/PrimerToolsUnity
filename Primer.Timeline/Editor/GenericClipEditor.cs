using System;
using Primer.Timeline.FakeUnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

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
        }

        private void ShowCodeEditorLink()
        {
            MonoBehaviour script = clip.template switch {
                ScrubbablePlayable a => null,
                TriggerablePlayable b => b.triggerable,
                SequencePlayable c => c.sequence,
                _ => throw new ArgumentOutOfRangeException()
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
    }
}
