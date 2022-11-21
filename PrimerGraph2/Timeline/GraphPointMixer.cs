using System.Collections.Generic;
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

            for (var i = 0; i < input.Length; i++) {
                points[i].localPosition = input.Points[i];
                points[i].localScale = Vector3.one;
            }

            if (fadeModifier > -1) {
                var tail = points[isFadeOut ? 0 : points.Count - 1];
                getAppearanceAnimator(isFadeOut).Evaluate(tail, fadeModifier);
            }
        }

        protected override ILine SingleInput(ILine input, float weight, bool isReverse) {
            var reduction = input.Length * weight;
            var newLength = Mathf.CeilToInt(reduction);
            var t = reduction.GetDecimals();

            isFadeOut = isReverse;
            fadeModifier = t;

            return input.Cut(newLength, isReverse);
        }

        protected override ILine Mix(List<float> weights, List<ILine> inputs) {
            // this tells Apply() we're not scaling any point
            fadeModifier = -1;

            // ILine.Lerp is going to resize the grids
            // But we calculate max size in advance so grids
            // only suffer a single transformation

            var maxPoints = 0;
            var count = inputs.Count;

            for (var i = 0; i < count; i++) {
                var length = inputs[i].Length;
                if (length > maxPoints) maxPoints = length;
            }

            var result = inputs[0].Resize(maxPoints);

            for (var i = 1; i < count; i++) {
                result = SimpleLine.Lerp(result, inputs[i], weights[i]);
            }

            return result;
        }

        void EnsurePointsCountMatches(GraphPoint target, ILine input) {
            if (points.Count > input.Length) {
                points.GetRange(input.Length, points.Count - input.Length).DisposeAll();
                points.RemoveRange(input.Length, points.Count - input.Length);
                return;
            }

            // var graph = target.GetComponentInParent<Graph2>();

            for (var i = points.Count; i < input.Length; i++) {
                var copy = Object.Instantiate(target.prefab, target.transform);
                copy.hideFlags = HideFlags.DontSave;
                points.Add(copy);
            }
        }
    }
}
