using UnityEngine;

namespace Primer.Timeline
{
    public class ChangeParent : Triggerable
    {
        public bool worldPositionStays = true;

        public Transform prevParent = null;
        public Transform newParent = null;

        public override void Cleanup()
        {
            base.Cleanup();
            transform.SetParent(prevParent, worldPositionStays);
        }

        public override void Prepare()
        {
            base.Prepare();
            transform.SetParent(newParent, worldPositionStays);
        }

        // This is required or clip will crash
        public void SomeEmptyMethod() {}
    }
}
