using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PrimerBlob : PrimerCharacter
{
    internal Color color = PrimerColor.Blue;
    internal List<BlobAccessory> accessories = new List<BlobAccessory>();

    // So many of these are global variables for two reasons.
    // (1) Releasing the override gradually toward the state of the animator is done in LateUpdate.
    // (2) I want to be able to change the focus without stopping a coroutine.
    // I don't doubt there's a cleaner way to achieve this. If you're reading this and think/know of one,
    // I'm all ears!
    private Transform visualFocus; 
    private float overrideStartTime;
    private Quaternion initialRot;
    private Vector3 lookCorrection;
    private bool overrideNeck = false;
    private bool releasing = false;
    private float releaseStartTime;
    private float releaseDuration;
    private Transform neckBone;
    private Quaternion neckOverrideRotation; //For setting in LateUpdate, not the initial orientation
    private Quaternion baseNeckRot; //Calculate this because it's arranged all wonky in the model
    System.Random rng;

    IEnumerator tiltLR;
    IEnumerator tiltFB;
    IEnumerator turnLR;
    float turnDuration = 0.5f;

    [SerializeField] PrimerObject lEye = null;
    [SerializeField] PrimerObject rEye = null;
    [SerializeField] PrimerObject mouth = null;

    // Meshes
    [SerializeField] Mesh highPolyMesh = null;
    [SerializeField] Mesh lowPolyMesh = null;
    [SerializeField] Mesh staticMesh = null;
    public enum MeshType {
        HighPolySkinned,
        LowPolySkinned,
        Static
    }

    protected override void Awake() {
        base.Awake();
        neckBone = transform.FindDeepChild("bone_neck").transform;
        //Undo whole blob rotation, then rotate to wherever the neck is in world space
        baseNeckRot = Quaternion.Inverse(transform.rotation) * neckBone.rotation;
        animator = GetComponent<Animator>();

        lEye = transform.FindDeepChild("eye_l").GetComponent<PrimerObject>();
        rEye = transform.FindDeepChild("eye_r").GetComponent<PrimerObject>();
        mouth = transform.FindDeepChild("mouth").GetComponent<PrimerObject>();
        
    }

    protected virtual void Start() {
        if (SceneManager.instance != null) {
            rng = SceneManager.sceneRandom2; // Makes it so the constant rng calls don't disrupt the sim's rng
        }
        else {
            rng = new System.Random(System.Environment.TickCount);
        }
    }

    void Update()
    {   
        //Just a moving a bunch of animation parameters

        //Random idle wiggles
        //Each blob will look different (if the rng seed is different)
        double changeProbability = 0.003;
        if (rng.NextDouble() < changeProbability) {
            this.TiltHeadLeftRight(((float)rng.NextDouble() * 2 - 1)*2);
        }
        if (rng.NextDouble() < changeProbability) {
            this.TiltHeadFrontBack(((float)rng.NextDouble() * 2 - 1)*2);
        }
        if (rng.NextDouble() < changeProbability) {
            this.TurnHeadLeftRight(((float)rng.NextDouble() * 2 - 1)*2);
        }

        double blinkProbability = 0.002;
        if (rng.NextDouble() < blinkProbability) {
            Blink();
        }
    }

    void LateUpdate() {
        if (overrideNeck) {
            //Seems we have to release here, since IEnumerators act before the animator updates
            //This is the only way I can see to interpolate between the animator state and the 
            //lookAt rotation to gradually release.
            if (!releasing) {
                // Debug.Log("Not releasing");
                neckBone.rotation = neckOverrideRotation; 
            }
            else { 
                if (Time.time < releaseStartTime + releaseDuration) { //Releasing
                    Quaternion goalRot = neckBone.rotation; //Wherever the animator has it
                    float t = (Time.time - releaseStartTime) / releaseDuration;
                    t = Helpers.ApplyNormalizedEasing(t, EaseMode.Cubic);
                    neckBone.rotation = Quaternion.Slerp(neckOverrideRotation, goalRot, t);
                }
                else { //We done
                    overrideNeck = false;
                    releasing = false;
                    // Debug.Log("Done releasing look");
                }
            }
        }
    }

    internal void SwapMesh(MeshType mType = MeshType.Static) {
        GameObject meshGO = transform.Find("blob_mesh").gameObject;
        // This only implements the switch to static, since that's the one I intend to use rn.
        SkinnedMeshRenderer smr = meshGO.GetComponent<SkinnedMeshRenderer>();
        switch (mType) {
            case MeshType.Static:
                animator.SetTrigger("Still");
                meshGO.AddComponent<MeshFilter>().mesh = staticMesh;
                meshGO.AddComponent<MeshRenderer>().material = smr.material;
                smr.enabled = false;
                break;
            case MeshType.LowPolySkinned:
                smr.sharedMesh = lowPolyMesh;
                smr.enabled = true;
                break;
            case MeshType.HighPolySkinned:
                smr.sharedMesh = highPolyMesh;
                smr.enabled = true;
                break;
            default:
                break;
        }
    }

    //TODO: This currently makes the blob appear to look a bit above the object, because of the model setup.
    //This looks natural for things below the horizon of the blob's eyes, since you assume it's moving its
    //eyeballs in addition to its neck. But for things above, it moves its neck too much and looks odd.
    //Fix this by correcting the facing vector to be truly straight-on with the blob model, and then add
    //another correction to make the blob move its head less than necessary, making it appear that some
    //eye movment is also happening.
    //For now, just manually defining a correction vector to get close.
    public override void StartLookingAt(Transform obj, float moveDuration = 0.5f, Vector3 correctionVector = new Vector3()) {
        overrideNeck = true;
        // Debug.Log("Starting to look");
        // Debug.Log(releasing);
        StartCoroutine(lookAt(obj, moveDuration, correctionVector));
    }

    public void ChangeFocus(Transform obj, Vector3 correctionVector = new Vector3()) {
        visualFocus = obj;
        lookCorrection = correctionVector;
        initialRot = neckBone.rotation; //The rotation set by lookAt
        overrideStartTime = Time.time;
    }

    public override void StopLooking(float duration = 0.5f) {
        releasing = true;
        releaseStartTime = Time.time;
        releaseDuration = duration;
    }

    private IEnumerator lookAt(Transform obj, float duration, Vector3 correctionVector) {
        visualFocus = obj;
        overrideStartTime = Time.time;
        initialRot = neckBone.rotation;
        lookCorrection = correctionVector;
        Quaternion goalRot;
        while (!releasing) {
            goalRot = Quaternion.LookRotation(visualFocus.position - neckBone.position + lookCorrection);
            float t = (Time.time - overrideStartTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, EaseMode.Cubic);
            neckOverrideRotation = Quaternion.Slerp(initialRot, goalRot * baseNeckRot, t);
            yield return null;
        }
    }

    void TiltHeadLeftRight(float val) {
        if (tiltLR != null) {
            StopCoroutine(tiltLR);
        }
        tiltLR = tiltHeadLeftRight(val);
        StartCoroutine(tiltLR);
    }
    IEnumerator tiltHeadLeftRight(float val) {
        float startTime = Time.time;
        float duration = turnDuration;
        float startVal = animator.GetFloat("tiltLeftRight");

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, EaseMode.Cubic);
            float nextVal = Mathf.Lerp(startVal, val, t);
            animator.SetFloat("tiltLeftRight", nextVal);
            yield return null;
        }
        animator.SetFloat("tiltLeftRight", val);
    }

    void TiltHeadFrontBack(float val) {
        if (tiltFB != null) {
            StopCoroutine(tiltFB);
        }
        tiltFB = tiltHeadFrontBack(val);
        StartCoroutine(tiltFB);
    }
    IEnumerator tiltHeadFrontBack(float val) {
        float startTime = Time.time;
        float duration = turnDuration;
        float startVal = animator.GetFloat("tiltFrontBack");

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, EaseMode.Cubic);
            float nextVal = Mathf.Lerp(startVal, val, t);
            animator.SetFloat("tiltFrontBack", nextVal);
            yield return null;
        }
        animator.SetFloat("tiltFrontBack", val);
    }

    void TurnHeadLeftRight(float val) {
        if (turnLR != null) {
            StopCoroutine(turnLR);
        }
        turnLR = turnHeadLeftRight(val);
        StartCoroutine(turnLR);
    }
    IEnumerator turnHeadLeftRight(float val) {
        float startTime = Time.time;
        float duration = turnDuration;
        float startVal = animator.GetFloat("turnLeftRight");

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, EaseMode.Cubic);
            float nextVal = Mathf.Lerp(startVal, val, t);
            animator.SetFloat("turnLeftRight", nextVal);
            yield return null;
        }
        animator.SetFloat("turnLeftRight", val);
    }

    void Blink() {
        //Could be global?
        double staggeredProbability = 0.2;
        float delay = 0.1f;

        double roll = rng.NextDouble();
        if (roll < staggeredProbability / 2) {
            LeftBlink();
            Invoke("RightBlink", delay);
        }
        else if (roll < staggeredProbability) {
            RightBlink();
            Invoke("LeftBlink", delay);
        }
        else {
            LeftBlink();
            RightBlink();
        }
    }
    void LeftBlink() {
            animator.SetTrigger("leftBlink");
    }
    void RightBlink() {
            animator.SetTrigger("rightBlink");
    }

    public override void SetColor(Color newColor) {
        Transform go = null;
        go = this.transform.Find("blob_mesh");
        if (go == null) {
            go = this.transform.Find("blob_mball.022");
        }
        if (go == null) {
            Debug.LogError("Couldn't find valid object for PrimerBlob.SetColor.");
        }
        Renderer r = go.GetComponent<Renderer>();
        base.SetColor(newColor, r);
        this.color = newColor;
    }

    public override void ChangeColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        Transform go = null;
        go = this.transform.Find("blob_mesh");
        if (go == null) {
            go = this.transform.Find("blob_mball.022");
        }
        if (go == null) {
            Debug.LogError("Couldn't find valid object for PrimerBlob.SetColor.");
        }
        Renderer r = go.GetComponent<Renderer>();
        base.ChangeColor(newColor, r, duration, ease);
        this.color = newColor;
    }
    public override void FadeOut(float newAlpha = 0, float duration = 0.5f, float delay = 0, EaseMode ease = EaseMode.None, List<PrimerObject> exemptions = null) {
        if (exemptions == null) { exemptions = new List<PrimerObject>(); }
        exemptions.Add(lEye);
        exemptions.Add(rEye);
        exemptions.Add(mouth);
        Debug.Log(exemptions.Count);
        base.FadeOut(newAlpha: newAlpha, duration: duration, delay: delay, ease: ease, exemptions: exemptions);
    }


    public void PulseEyes(float factor = 1.2f, float duration = 1, float attack = 0.5f, float decay = 0.5f) {
        lEye.Pulse(factor, duration, attack, decay);
        rEye.Pulse(factor, duration, attack, decay);
    }

    public override void Wave(float duration = 2, bool smile = false) {
        StartCoroutine(wave(duration, smile));
    }
    IEnumerator wave(float duration, bool smile) {
        animator.SetTrigger("Wave");
        if (smile) {animator.SetTrigger("MouthSmile");}
        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Wiggles");
        if (smile) {animator.SetTrigger("MouthClosed");}
    }

    public void BigEyes() {
        animator.SetTrigger("LeftEyeBig");
        animator.SetTrigger("RightEyeBig");
    }
    public void NeutralEyes() {
        animator.SetTrigger("LeftEyeNeutral");
        animator.SetTrigger("RightEyeNeutral");
    }
    public void EffortEyes() {
        animator.SetTrigger("EffortLeft");
        animator.SetTrigger("EffortRight");
    }

    public void CloseEyes(float duration = 1) {
        StartCoroutine(closeEyes(duration));
    }
    IEnumerator closeEyes(float duration) {
        animator.SetTrigger("leftBlink");
        animator.SetBool("leftHoldClosed", true);
        animator.SetTrigger("rightBlink");
        animator.SetBool("rightHoldClosed", true);
        yield return new WaitForSeconds(duration);
        animator.SetBool("leftHoldClosed", false);
        animator.SetBool("rightHoldClosed", false);
    }

    public void Panic(float duration = 2, bool mouthOpen = true) {
        StartCoroutine(panic(duration, mouthOpen));
    }
    IEnumerator panic(float duration, bool mouthOpen) {
        EffortEyes();
        animator.SetBool("panicking", true);
        if (mouthOpen) {
            animator.SetTrigger("MouthOpenWide");
        }
        yield return new WaitForSeconds(duration);
        NeutralEyes();
        animator.SetBool("panicking", false);
        if (mouthOpen) {
            animator.SetTrigger("MouthClosed");
        }
    }

    internal BlobAccessory AddAccessory(AccessoryType accessoryType, bool animate = false, bool highQuality = true, bool colorMatch = false) {
        BlobAccessory accessory = BlobAccessory.NewAccessory(this, accessoryType, highQuality: highQuality, colorMatch: colorMatch);
        if (animate) {
            if (accessory != null) {
                accessory.ScaleUpFromZero();
            }
        }
        accessories.Add(accessory);
        return accessory;
    }
    // internal void HideBeard() {
    //     if (beard != null) {
    //         beard.ScaleDownToZero();
    //     }
    // }
}
