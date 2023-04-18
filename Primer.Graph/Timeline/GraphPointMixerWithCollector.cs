using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Primer.Timeline;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphPointMixerWithCollector : PrimerMixerWithCollector<GraphPoint, ILine>
    {
        public Func<bool, PrimerAnimator> getAppearanceAnimator;
        private float fadeModifier;
        private bool isFadeOut;

        protected override void Stop()
            => ChildrenDeclaration.Clear(trackTarget.transform);

        protected override IMixerCollector<ILine> CreateCollector()
        {
            return new CollectorWithDirection<PrimerPlayable, ILine>(
                behaviour =>
                    behaviour is ILineBehaviour { Points: {} } lineBehaviour
                        ? lineBehaviour.Points
                        : null
            );
        }

        protected override void Frame(IMixerCollector<ILine> genericCollector)
        {
            var collector = (CollectorWithDirection<PrimerPlayable, ILine>)genericCollector;

            var state = collector.count > 1
                ? Mix(collector.weights, collector.inputs)
                : collector.isFull
                    ? collector[0].input
                    : CutLine(collector[0].input, collector[0].weight, collector.isReverse);

            ApplyState(state);
        }

        protected ILine CutLine(ILine input, float weight, bool isReverse)
        {
            var reduction = input.Segments * weight;
            var newLength = Mathf.CeilToInt(reduction);
            var t = reduction.GetDecimals();

            isFadeOut = isReverse;
            fadeModifier = t;

            return input.Crop(newLength, isReverse);
        }

        protected ILine Mix(IReadOnlyList<float> weights, IReadOnlyList<ILine> inputs)
        {
            // this tells Apply() we're not scaling any point
            fadeModifier = -1;

            var lines = ILine.Resize(inputs.ToArray());
            var result = lines[0];

            for (var i = 1; i < lines.Length; i++)
                result = ILine.Lerp(result, lines[i], weights[i]);

            return result;
        }

        private void ApplyState(ILine input)
        {
            var positionMultiplier = trackTarget.GetPositionMultiplier();
            var scale = trackTarget.GetScaleNeutralizer();

            var children = new ChildrenDeclaration(trackTarget.transform);

            var points = input.Points.Select(
                    (position, i) => {
                        var point = children.NextIsInstanceOf(
                            trackTarget.prefab,
                            $"Point {i}",
                            init: x => x.hideFlags = HideFlags.DontSave
                        );

                        point.localPosition = Vector3.Scale(position, positionMultiplier);
                        point.localScale = scale;
                        return point;
                    }
                )
                .ToList();

            children.Apply();

            if (fadeModifier < 0)
                return;

            var tail = isFadeOut ? points.First() : points.Last();
            getAppearanceAnimator(isFadeOut).Evaluate(tail, fadeModifier);
        }
    }
}
