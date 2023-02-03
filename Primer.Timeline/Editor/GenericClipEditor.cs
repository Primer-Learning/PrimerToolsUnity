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
            if (clip.trackTarget is null)
                FillTrackTarget();

            base.OnInspectorGUI();
        }

        private void FillTrackTarget()
        {
            var director = TimelineEditor.inspectedDirector;
            var track = director.GetTrackOfClip(clip);
            var bounds = director.GetGenericBinding(track);

            if (bounds is Transform transform)
                clip.trackTarget = transform;
        }
    }
}
