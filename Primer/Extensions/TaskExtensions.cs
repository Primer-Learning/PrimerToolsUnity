using System;
using System.Threading.Tasks;

namespace Primer
{
    public static class TaskExtensions
    {
        // TODO: Remove this method
        [Obsolete("Use UniTask instead of Task. It contains a .Forget() method that does the same thing as this method.")]
        public static void FireAndForget(this Task task) {}
    }
}
