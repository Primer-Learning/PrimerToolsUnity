using System;
using Primer.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Animation.Editor
{
    public static class AnimationCreateUtility
    {
        [MenuItem("GameObject/Primer animation/Scale up from zero", true, CreateUtility.PRIORITY)]
        public static bool ScaleUpFromZero_Validate(MenuCommand command) =>
            Selection.activeGameObject != null;

        [MenuItem("GameObject/Primer animation/Scale up from zero", false, CreateUtility.PRIORITY)]
        public static void ScaleUpFromZero(MenuCommand command)
        {
            // Get the selected element and it's animator
            if (command.context is not GameObject selected) {
                throw new Exception("This should never happen");
            }

            var anim = selected.GetOrAddComponent<Animator>();

            // Get the active timeline
            var director = TimelineEditor.inspectedDirector;
            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null) throw new Exception("POTATO");

            // Create the animation track
            var track = timeline.CreateTrack<AnimationTrack>();
            director.SetGenericBinding(track, anim);
            track.CreateInfiniteClip("Scale up from zero");

            // values we gonna need
            var clip = track.infiniteClip;
            var time = (float)director.time;
            var scale = selected.transform.localScale;

            // What are we going to animate
            var xBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x");
            var yBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y");
            var zBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z");

            // What will the values be
            var startAsZero = new Keyframe(time, 0);
            var endTime = time + 0.5f;
            var xCurve = new AnimationCurve(startAsZero, new Keyframe(endTime, scale.x));
            var yCurve = new AnimationCurve(startAsZero, new Keyframe(endTime, scale.y));
            var zCurve = new AnimationCurve(startAsZero, new Keyframe(endTime, scale.z));

            // Apply the keyframes to the clip
            AnimationUtility.SetEditorCurve(clip, xBinding, xCurve);
            AnimationUtility.SetEditorCurve(clip, yBinding, yCurve);
            AnimationUtility.SetEditorCurve(clip, zBinding, zCurve);

            // Update timeline window
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }
}
