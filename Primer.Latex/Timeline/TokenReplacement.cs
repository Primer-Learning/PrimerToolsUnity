using System;
using System.Text.RegularExpressions;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Latex
{
    [Serializable]
    public class TokenReplacement : PrimerMixer<LatexRenderer>
    {
        private LatexInput originalConfig;
        public string searchToken = "";
        public string replaceWith = "";

        protected override void Start(LatexRenderer latex)
        {
            originalConfig = latex.Config;
            Debug.Log("START");
        }
        protected override void Stop(LatexRenderer latex)
        {
            _ = latex.Render(originalConfig);
            Debug.Log("STOP");
        }
        protected override void Frame(LatexRenderer latex, Playable playable, FrameData info)
        {
            Debug.Log($"GOTTA {searchToken}");

            var replacer = new Regex(searchToken);
            var newLatex = replacer.Replace(originalConfig.Latex, replaceWith);
            var newConfig = latex.Config with {Latex = newLatex};

            if (latex.Config.Equals(newConfig)) return;

            _ = latex.Render(newConfig);


            // I don't remember how to do this without mixer
            // ask ChatGPT
            // re-render latex only once
            // when that is working add animation
        }
    }
}
