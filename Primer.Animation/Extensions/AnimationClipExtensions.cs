using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Animation
{
    public static class AnimationClipExtensions
    {
        public static void AddCurves<T>(this AnimationClip clip, Dictionary<string, AnimationCurve> boundCurves) where T : Component
        {
            var type = typeof(T);
            var input = new Dictionary<string, AnimationCurve>(boundCurves);
            var bindings = new List<EditorCurveBinding>();
            var curves = new List<AnimationCurve>();

            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);

                if (binding.type == type && input.ContainsKey(binding.propertyName)) {
                    MergeCurves(curve, input[binding.propertyName]);
                    input.Remove(binding.propertyName);
                }

                bindings.Add(binding);
                curves.Add(curve);
            }

            // Add any curves not present yet
            bindings.AddRange(input.Keys.Select(prop => EditorCurveBinding.FloatCurve("", type, prop)));
            curves.AddRange(input.Values);

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }

        public static void MakeCubicCurves(this AnimationClip clip)
        {
            var bindings = new List<EditorCurveBinding>();
            var curves = new List<AnimationCurve>();

            foreach (var binding in AnimationUtility.GetCurveBindings(clip)) {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                curves.Add(IPrimerAnimation.CubicCurve(curve));
                bindings.Add(binding);
            }

            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curves.ToArray());
        }

        public static bool IsRecordedAndPristine(this AnimationClip clip)
        {
            if (!clip.name.StartsWith("Recorded"))
                return false;

            return AnimationUtility.GetCurveBindings(clip)
                .Select(binding => AnimationUtility.GetEditorCurve(clip, binding))
                .SelectMany(curve => curve.keys)
                .All(key => key.weightedMode == WeightedMode.None);
        }

        private static void MergeCurves(AnimationCurve target, AnimationCurve source)
        {
            for (var i = 0; i < source.length; i++)
                target.AddKey(source[i]);
        }
    }
}
