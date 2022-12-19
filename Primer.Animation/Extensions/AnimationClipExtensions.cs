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

        private static void MergeCurves(AnimationCurve target, AnimationCurve source)
        {
            for (var i = 0; i < source.length; i++)
                target.AddKey(source[i]);
        }
    }
}
