using System;

namespace Primer.Latex
{
    public class LatexTransitionGroupAttribute : Attribute
    {
        public string from;
        public string to;

        public LatexTransitionGroupAttribute(string from, string to)
        {
            this.from = from;
            this.to = to;
        }
    }
}
