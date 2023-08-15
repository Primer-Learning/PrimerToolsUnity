using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [DisallowMultipleComponent]
    public abstract class Sequence : AsyncMonoBehaviour
    {
        private readonly List<Transform> disposeOnCleanup = new();

        public virtual void Cleanup()
        {
            disposeOnCleanup.Dispose();
            disposeOnCleanup.Clear();
        }

        protected void DisposeOnCleanup(Component child)
        {
            disposeOnCleanup.Add(child.transform);
            SequenceOrchestrator.EnsureDisposal(child);
        }

        public SequenceRunner Run()
        {
            ClearClipColor();
            return new SequenceRunner(this);
        }

        // TODO: Make protected
        public abstract IAsyncEnumerator<Tween> Define();

        #region Clip color
        internal Stack<Color> clipColors = new();
        public Color clipColor => clipColors.Count > 0 ? clipColors.Peek() : Color.clear;

        protected void PushClipColor(Color newColor) => clipColors.Push(newColor);
        protected void PopClipColor() => clipColors.Pop();

        protected void ClearClipColor() => clipColors.Clear();
        protected void SetClipColor(Color newColor)
        {
            ClearClipColor();
            PushClipColor(newColor);
        }
        #endregion
    }
}
