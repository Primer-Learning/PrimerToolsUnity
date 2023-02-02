using System;

namespace Primer.Timeline
{
    public class FailedClipException : Exception
    {
        public FailedClipException() : base("Refusing to execute failing clip") {}
    }
}
