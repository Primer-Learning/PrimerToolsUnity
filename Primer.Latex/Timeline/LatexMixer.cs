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

        /// <summary>
        ///     For LaXex track `input` can't be more than two items:
        ///      - gameObject's characters
        ///      - the characters modified by a clip
        ///
        ///     each one with it's weights
        ///     this function does the magic of transition between them
        /// </summary>
        protected override LatexChar[] Mix(List<float> weights, List<LatexChar[]> inputs)
        {
            var setsToMerge = inputs.Select(x => x.ToList()).ToList();
            var common = FindCommonSymbols(inputs);
            var result = new List<LatexChar>();

            // iterate over every symbol the `inputs` share
            // if a symbol is present twice on each `input` it'll also be twice in `common`
            foreach (var symbol in common) {
                LatexChar lerped = null;

                for (var i = 0; i < setsToMerge.Count; i++) {
                    var set = setsToMerge[i];

                    // sometimes character is not found
                    // this shouldn't happen
                    // error in the logic?
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

            var unique = GetUniqueSymbols(symbols[0]);
            var common = new List<LatexSymbol>();

            foreach (var character in unique) {
                var repetitions = symbols.Min(chars =>  chars.Count(x => x.Equals(character)));

                for (var i = 0; i < repetitions; i++) {
                    common.Add(character);
                }
            }

            return common;
        }

        private static List<LatexSymbol> GetUniqueSymbols(IReadOnlyList<LatexSymbol> list)
        {
            var unique = new List<LatexSymbol>();

            for (var i = 0; i < list.Count; i++) {
                var character = list[i];

                if (!unique.Any(x => x.Equals(character))) {
                    unique.Add(character);
                }
            }

            return unique;
        }
    }
}
