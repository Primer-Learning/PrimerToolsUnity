using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimerCharacter : PrimerObject
{
    // Currently a replacement for the blobs, which I'm keeping private
    internal Animator animator = null;
    public virtual Animator GetAnimator() {
        return this.GetComponent<Animator>();
    }
    public virtual void StartLookingAt(Transform obj, float moveDuration = 0.5f, Vector3 correctionVector = new Vector3()) {}
    public virtual void StopLooking(float duration = 0.5f) {}
    public virtual void Wave(float duration = 0.5f, bool smile = false) {}
}
