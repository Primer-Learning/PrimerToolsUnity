using UnityEngine;

namespace Primer.Timeline
{
    /// <summary>
    /// When we press one of these "View in play mode" buttons the C# scripts get re-compiled
    ///  any running function gets interrupted and awaits are cancelled.
    ///
    /// This is why we need to save (serialize) a component to resume the function after the re-compile.
    /// </summary>
    public abstract class PlayModeBehaviour : MonoBehaviour
    {
        public bool runOnlyOnce = true;

        public void EnterPlayMode()
        {
            if (!Application.isPlaying)
                PrimerTimeline.EnterPlayMode();
        }

        public void Execute()
        {
            try {
                Action();
            }
            finally {
                if (runOnlyOnce)
                    Remove();
            }
        }

        public void Remove()
        {
            PrimerTimeline.onEnterPlayMode -= Execute;
            transform.Dispose();
        }

        protected abstract void Action();

        private void OnEnable()
        {
            PrimerTimeline.onEnterPlayMode += Execute;
        }
    }
}
