using UnityEngine;

namespace Primer.Timeline
{
    public class ChangeParent : Triggerable
    {
        public Transform prevParent = null;
        public Transform newParent = null;

        public override void Cleanup()
        {
            base.Cleanup();
            transform.parent = prevParent;
        }

        public override void Prepare()
        {
            base.Prepare();
            transform.parent = newParent;
        }

        // This is required or clip will crash
        public void SomeEmptyMethod() {}
    }
}
