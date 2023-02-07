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
