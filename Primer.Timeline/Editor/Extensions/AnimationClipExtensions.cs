using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    public static class AnimationClipExtensions
    {
            public static IEnumerable<Keyframe[]> IterateAllKeyframes(this AnimationClip clip)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Select(binding => AnimationUtility.GetEditorCurve(clip, binding))
                .Select(curve => curve.keys);
        }

        public static void ModifyKeyframes(this AnimationClip clip, Func<Keyframe, Keyframe> modifier)
        {
            var bindings = new List<EditorCurveBinding>();
            var curves = new List<AnimationCurve>();

            foreach (var binding in AnimationUtility.GetCurveBindings(clip)) {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                curves.Add(new AnimationCurve(curve.keys.Select(modifier).ToArray()));
                bindings.Add(binding);
            }

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }
    }
}
