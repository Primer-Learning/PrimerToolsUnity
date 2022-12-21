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

        protected override void Stop(LatexRenderer trackTarget)
        {
            trackTarget.onChange.RemoveListener(UpdateCharacters);
            trackTarget.characters = originalValue;
        }

        private void UpdateCharacters(LatexChar[] newChars) =>
            originalValue = newChars;
        #endregion

        protected override LatexChar[] ProcessPlayable(PrimerPlayable behaviour)
        {
            return behaviour is ILatexCharProvider {isReady: true} chars
                ? chars.characters
                : null;
        }

        protected override LatexChar[] SingleInput(LatexChar[] input, float weight, bool isReverse)
        {
            return weight == 1
                ? input
                : input.Select(x => LatexChar.LerpScale(x, weight)).ToArray();
        }

        protected override void Apply(LatexRenderer trackTarget, LatexChar[] input)
        {
            trackTarget.characters = input;
        }

        protected override LatexChar[] Mix(List<float> weights, List<LatexChar[]> inputs)
        {
            var setsToMerge = inputs.Select(x => x.ToList()).ToList();
            var common = FindCommonSymbols(inputs);
            var result = new List<LatexChar>();

            foreach (var symbol in common) {
                LatexChar lerped = null;

                for (var i = 0; i < setsToMerge.Count; i++) {
                    var set = setsToMerge[i];

                    var character = set.Find(x => x.symbol.Equals(symbol));
                    set.Remove(character);

                    lerped = lerped is null
                        ? character
                        : LatexChar.Lerp(lerped, character, weights[i]);
                }

                result.Add(lerped);
            }

            for (var i = 0; i < setsToMerge.Count; i++) {
                var lerpedSet = setsToMerge[i].Select(character => LatexChar.LerpScale(character, weights[i]));
                result.AddRange(lerpedSet);
            }

            return result.ToArray();
        }

        private static List<LatexSymbol> FindCommonSymbols(IEnumerable<LatexChar[]> inputs)
        {
            var symbols = inputs
                .Select(input => input.Select(character => character.symbol).ToList())
                .ToList();

            var first = symbols[0].ToHashSet();
            var common = new List<LatexSymbol>();

            foreach (var character in first) {
                var repetitions = symbols.Min(chars =>  chars.Count(x => x.Equals(character)));

                for (var i = 0; i < repetitions; i++) {
                    common.Add(character);
                }
            }

            return common;
        }
    }
}
