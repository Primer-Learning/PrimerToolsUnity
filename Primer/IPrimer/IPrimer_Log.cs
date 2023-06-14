#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_LogExtensions
    {
        public static void Log(this IPrimer self, params object[] data)
        {
            PrimerLogger.Log(self.transform, data);
        }

        public static void Error(this IPrimer self, Exception exception)
        {
            PrimerLogger.Error(self.transform, exception);
        }

        public static void Log(this Component self, params object[] data)
        {
            PrimerLogger.Log(self, data);
        }

        public static void Error(this Component self, Exception exception)
        {
            PrimerLogger.Error(self, exception);
        }
    }
}
#endif
