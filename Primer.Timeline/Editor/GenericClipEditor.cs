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
            if (clip.template is PrimerPlayable<Transform> x && x.trackTarget == null) {
                FillTrackTarget(x);
            }

            base.OnInspectorGUI();
        }

        private void FillTrackTarget(PrimerPlayable<Transform> behaviour)
        {
            var director = TimelineEditor.inspectedDirector;
            var track = director.GetTrackOfClip(clip);
            var bounds = director.GetGenericBinding(track);

            if (bounds is Transform transform)
                behaviour.trackTarget = transform;
        }
    }
}
