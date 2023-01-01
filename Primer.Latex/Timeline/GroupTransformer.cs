using System;
using System.Collections.Generic;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class GroupTransformer : PrimerPlayable
    {
        [HideInInspector] internal LatexRenderer transformTo;
        [HideInInspector] internal List<TransitionType> transitions;
    }
}
