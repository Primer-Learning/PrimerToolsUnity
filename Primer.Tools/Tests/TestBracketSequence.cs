using System.Collections.Generic;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Tools.Tests
{
    public class TestBracketSequence : Sequence
    {
        public PrimerBracket2 bracket;

        public override async IAsyncEnumerator<object> Run()
        {
            bracket.width = 1;
            bracket.anchor = new Vector3(0, 1, 0);
            bracket.left = new Vector3(0, 0, 2);
            bracket.right = new Vector3(0, 0, -2);

            yield return null;

            bracket.width = 2;
            bracket.anchor = new Vector3(1, 0, 0);
            bracket.left = new Vector3(0, 0, 2);
            bracket.right = new Vector3(0, 0, -2);

            yield return null;
        }
    }
}
