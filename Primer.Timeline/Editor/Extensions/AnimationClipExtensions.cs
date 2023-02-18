using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    public static class AnimationClipExtensions
    {
        public static void ModifyKeyFrames(this AnimationClip clip, Func<Keyframe, Keyframe> modifier)
        {
            var bindings = new List<EditorCurveBinding>();
            var curves = new List<AnimationCurve>();

            foreach (var binding in AnimationUtility.GetCurveBindings(clip)) {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                curves.Add(new AnimationCurve(curve.keys.Select(modifier).ToArray()));
                bindings.Add(binding);
            }

            try {
                AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
            }
            catch (Exception e) {
                // ignore
            }
        }
    }
}
