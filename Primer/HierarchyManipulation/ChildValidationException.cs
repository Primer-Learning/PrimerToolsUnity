using System;

namespace Primer
{
    internal class ChildValidationException : Exception
    {
        public ChildValidationException(string message) : base(message) {}
    }
}
