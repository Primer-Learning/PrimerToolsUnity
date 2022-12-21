using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Latex
{
    public class LatexMixer : CollectedMixer<LatexRenderer, LatexChar[]>
    {
        #region Save and restore original characters
        protected override void Start(LatexRenderer trackTarget)
        {
            originalValue = trackTarget.characters;
            trackTarget.onChange.AddListener(UpdateCharacters);
        }

        protected override void Stop(LatexRenderer trackTarget) =>
            trackTarget.onChange.RemoveListener(UpdateCharacters);

        private void UpdateCharacters(LatexChar[] newChars) =>
            originalValue = newChars;
        #endregion

        protected override LatexChar[] ProcessPlayable(PrimerPlayable behaviour) =>
            behaviour is ILatexCharProvider {isReady: true} chars
                ? chars.characters
                : null;

        protected override LatexChar[] SingleInput(LatexChar[] input, float weight, bool isReverse) =>
            input;

        protected override LatexChar[] Mix(List<float> weights, List<LatexChar[]> inputs)
        {
            // var presentInAll = inputs.
            var hashes = inputs.Select(x => x.ToHashSet()).ToArray();
            var firstHash = hashes[0];
            var rest = hashes.Skip(1).ToArray();
            var common = new List<LatexChar>();

            foreach (var myChar in firstHash) {
                if (rest.All(x => x.Contains(myChar))) {
                    common.Add(myChar);
                }
            }

            var a = inputs[0][0];
            var b = inputs[1][0];

            var equal = a.IsSameGeometry(b);

            Debug.Log("Common");
            return common.ToArray();
        }

        protected override void Apply(LatexRenderer trackTarget, LatexChar[] input)
        {
            trackTarget.characters = input;
        }
    }
}
