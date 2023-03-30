using UnityEngine;

namespace Primer.Timeline
{
    public class ChangeParent : Triggerable
    {
        public bool worldPositionStays = true;
        
        // Long explanation for the setOriginalTransformDuringCleanup flag:
        // Fast scrubbing can disrupt the initial transform values if the new parent is moving (which is the point of this script)
        // To fix this, we need to keep track of the original transformation and restore it during cleanup.
        // But that original transformation needs to be stored somehow.
        // There could be a few approaches to storing the transformation. Ideal behavior is probably to watch for the 
        // transform to change in the editor while the playhead is before this triggerable. That way, you would never 
        // have to think about setting the transform. But I don't know how to do that.
        // Another way could be to use a button that you just click when you want to store the transform.
        // But I don't know how to do that either.
        // So I am using a bool. When the bool is true, the values will be stored during cleanup. And the way to use it
        // is to move the playhead before the triggerable, set the transform where you want it, click the bool
        // move the playhead a little bit to trigger cleanup and store the values, then uncheck the box.
        public bool setOriginalTransformDuringCleanup = false;

        public Transform prevParent = null;
        public Transform newParent = null;

        public  Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
        public Vector3 originalLocalScale;

        public override void Cleanup()
        {
            base.Cleanup();
            transform.SetParent(prevParent, worldPositionStays);
            if (setOriginalTransformDuringCleanup)
            {
                originalLocalPosition = transform.localPosition;
                originalLocalRotation = transform.localRotation;
                originalLocalScale = transform.localScale;
            }
            transform.localPosition = originalLocalPosition;
            transform.localRotation = originalLocalRotation;
            transform.localScale = originalLocalScale;
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
