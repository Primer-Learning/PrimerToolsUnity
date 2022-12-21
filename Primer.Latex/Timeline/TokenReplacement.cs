using System;
using System.Text.RegularExpressions;
using Primer.Timeline;
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
        }

        protected override void Stop(LatexRenderer latex)
        {
            latex.Render(originalConfig).FireAndForget();
        }

        protected override void Frame(LatexRenderer latex, Playable playable, FrameData info)
        {
            var replacer = new Regex(searchToken);
            var newLatex = replacer.Replace(originalConfig.Latex, replaceWith);
            var newConfig = latex.Config with {Latex = newLatex};

            if (!latex.Config.Equals(newConfig)) {
                latex.Render(newConfig).FireAndForget();
            }
        }
    }
}
