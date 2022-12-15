using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphPointMixer : CollectedMixer<GraphPoint, ILine>
    {
        public Func<bool, PrimerAnimator> getAppearanceAnimator;
        readonly List<Transform> points = new();
        float fadeModifier;
        bool isFadeOut;

        protected override void Start(GraphPoint target) {}

        protected override void Stop(GraphPoint target) {
            points.DisposeAll();
            points.Clear();
        }

        protected override ILine ProcessPlayable(PrimerPlayable behaviour) =>
            behaviour is ILineBehaviour {Points: {}} lineBehaviour
                ? lineBehaviour.Points
                : null;

        protected override void Apply(GraphPoint target, ILine input) {
            EnsurePointsCountMatches(target, input);

            for (var i = 0; i < input.Points.Length; i++) {
                points[i].localPosition = input.Points[i];
                points[i].localScale = Vector3.one;
            }

            if (fadeModifier > -1) {
                var tail = points[isFadeOut ? 0 : points.Count - 1];
                getAppearanceAnimator(isFadeOut).Evaluate(tail, fadeModifier);
            }
        }

        protected override ILine SingleInput(ILine input, float weight, bool isReverse) {
            var reduction = input.Segments * weight;
            var newLength = Mathf.CeilToInt(reduction);
            var t = reduction.GetDecimals();

            isFadeOut = isReverse;
            fadeModifier = t;

            return input.Crop(newLength, isReverse);
        }

        protected override ILine Mix(List<float> weights, List<ILine> inputs) {
            // this tells Apply() we're not scaling any point
            fadeModifier = -1;

            var lines = ILine.Resize(inputs.ToArray());
            var result = lines[0];

            for (var i = 1; i < lines.Length; i++) {
                result = ILine.Lerp(result, lines[i], weights[i]);
            }

            return result;
        }

        void EnsurePointsCountMatches(GraphPoint target, ILine input) {
            var inputLength = input.Points.Length;

            if (points.Count > inputLength) {
                points.GetRange(inputLength, points.Count - inputLength).DisposeAll();
                points.RemoveRange(inputLength, points.Count - inputLength);
                return;
            }

            for (var i = points.Count; i < inputLength; i++) {
                var copy = Object.Instantiate(target.prefab, target.transform);
                copy.hideFlags = HideFlags.DontSave;
                points.Add(copy);
            }
        }
    }
}
