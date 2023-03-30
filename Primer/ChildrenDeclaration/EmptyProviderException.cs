using System;

namespace Primer
{
    public class EmptyProviderException : Exception
    {
        public EmptyProviderException() : base("Prefab provider is empty") {}
    }
}
