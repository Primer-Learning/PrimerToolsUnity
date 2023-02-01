// using Primer.Animation;
// using UnityEditor;
// using UnityEditor.Timeline;
// using UnityEngine;
//
// namespace Primer.Timeline.Editor
// {
//     [CustomEditor(typeof(GenericBehaviour))]
//     public class GenericClipEditor : UnityEditor.Editor
//     {
//         private GenericClip clip => (GenericClip)target;
//
//         public override void OnInspectorGUI()
//         {
//             var director = TimelineEditor.inspectedDirector;
//             var track = director.GetTrackOfClip(clip);
//             var bounds = director.GetGenericBinding(track);
//
//             if (bounds is Transform transform && clip.template is PrimerPlayable<Transform> behaviour)
//                 behaviour.trackTarget = transform;
//
//             base.OnInspectorGUI();
//         }
//     }
// }
