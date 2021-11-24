using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSceneDirector : Director
{
    [SerializeField] PrimerCharacter blobCreaturePrefab = null;
    [SerializeField] GameObject ground = null;
    PrimerCharacter blobCreature;
    protected override void Awake() {
        base.Awake();
        //Do initialization stuff here
        camRig.SetUp(solidColor: false);
        camRig.GoToStandardPositions();
    }

    IEnumerator Appear() {
        // This is an example scene block.
        // Technically, it's a coroutine that is used 
        // to create a scene block in DefineSchedule() below,
        // but conceptually, this is where you define 
        // a scene block.

        blobCreature = Instantiate(blobCreaturePrefab, ground.transform);
        blobCreature.transform.localPosition = Vector3.zero;
        blobCreature.transform.localRotation = Quaternion.Euler(0, 180, 0);
        blobCreature.transform.localScale = Vector3.zero;
        blobCreature.ScaleUpFromZero();
        yield return new WaitForSeconds(0.5f);
        blobCreature.Wave(duration: 2, smile: true);
        
        yield return null;
    }

    //Define event actions
    IEnumerator Zoom() { 
        camRig.ZoomTo(20f, duration: 4); 
        yield return null;
    }

    IEnumerator MoveAndColor() {
        //Wiggle
        blobCreature.MoveTo(new Vector3(2f, 0, -2), duration: 1f);
        yield return new WaitForSeconds(2f);
        blobCreature.MoveTo(new Vector3(-2f, 0, -3), duration: 1f);
        yield return new WaitForSeconds(2f);
        blobCreature.MoveTo(new Vector3(0f, 0, 0), duration: 1f);
        yield return new WaitForSeconds(2f);

        //Walk around and change color
        blobCreature.WalkTo(new Vector3(3, 0, -3), duration: 1);
        yield return new WaitForSeconds(2);
        // MeshRenderer mr = blobCreature.transform.Find("body").GetComponent<MeshRenderer>();
        // Material mat = mr.material;
        // StartCoroutine(blobCreature.changeColor(mat, new Color(1, 0, 0, 1), duration: 0.5f, ease: EaseMode.None));
        blobCreature.ChangeColor(new Color(1, 0, 0, 1));
        blobCreature.WalkTo(new Vector3(-3, 0, 3), duration: 1);
        yield return new WaitForSeconds(2);
        blobCreature.WalkTo(new Vector3(-3, 0, -3), duration: 1);
        yield return new WaitForSeconds(2);
    }

    IEnumerator Disappear() {
        blobCreature.ScaleDownToZero();
        yield return new WaitForSeconds(1);
    }

    //Construct schedule
    protected override void DefineSchedule() {
        /*
        If flexible is true, blocks run as long (or not) as they need to. Then,
        the next block starts on the next frame and all later block timings
        are shifted by the same amount.
        Useful for simulations whose duration is not predetermined.
        */
        new SceneBlock(0f, Appear);
        new SceneBlock(3f, Zoom);
        new SceneBlock(5f, MoveAndColor, flexible: true);
        new SceneBlock(7f, Disappear);
    }
}
