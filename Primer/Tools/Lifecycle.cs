using System;

namespace Primer
{
    public class Lifecycle
    {
        private bool isInitialized;
        private int locks = 0;

        private readonly Action initialize;
        private readonly Action reset;

        public Lifecycle(Action initialize, Action reset)
        {
            this.initialize = initialize;
            this.reset = reset;
        }

        public IDisposable PreventInitialization()
        {
            return new InitializationLock(this);
        }


        public bool Initialize()
        {
            if (isInitialized || locks != 0)
                return false;

            initialize();
            isInitialized = true;
            return true;
        }

        public bool Reset()
        {
            if (!isInitialized)
                return false;

            reset();
            isInitialized = false;
            return true;
        }


        private class InitializationLock : IDisposable
        {
            private readonly Lifecycle target;

            public InitializationLock(Lifecycle target)
            {
                this.target = target;
                target.locks++;
            }

            public void Dispose()
            {
                target.locks--;
            }
        }
    }
}
