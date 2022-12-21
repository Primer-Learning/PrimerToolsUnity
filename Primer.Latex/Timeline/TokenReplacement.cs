using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Latex
{
    [Serializable]
    public class TokenReplacement : PrimerBoundPlayable<LatexRenderer>, ILatexCharProvider
    {
        internal LatexProcessor processor;
        [CanBeNull] private LatexInput lastUsedConfig;

        public string searchToken = "";
        public string replaceWith = "";


        public bool isReady { get; private set; } = false;
        public LatexChar[] characters { get; private set; }

        protected override void Start(LatexRenderer trackTarget) {}

        protected override void Stop(LatexRenderer trackTarget) {}

        protected override async void Frame(LatexRenderer trackTarget, Playable playable, FrameData info)
        {
            var originalFormula = trackTarget.config.code;
            var replacer = new Regex(searchToken);
            var newFormula = replacer.Replace(originalFormula, replaceWith);
            var newConfig = trackTarget.config with {code = newFormula};

            if (lastUsedConfig is not null && lastUsedConfig.Equals(newConfig))
                return;

            lastUsedConfig = newConfig;
            characters = await processor.Process(newConfig);
            isReady = true;
        }
    }
}
