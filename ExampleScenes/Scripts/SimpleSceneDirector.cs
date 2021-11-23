using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSceneDirector : Director
{
    [SerializeField] PrimerObject ballCreaturePrefab = null;
    [SerializeField] GameObject ground = null;
    PrimerObject ballCreature;
    protected override void Awake() {
        base.Awake();
        //Do initialization stuff here
        camRig.SetUp(solidColor: false);
        camRig.GoToStandardPositions();
    }

    IEnumerator Appear() {
        ballCreature = Instantiate(ballCreaturePrefab, ground.transform);
        ballCreature.transform.localPosition = Vector3.zero;
        ballCreature.transform.localRotation = Quaternion.Euler(0, 180, 0);
        ballCreature.transform.localScale = Vector3.zero;
        ballCreature.ScaleUpFromZero();
        yield return null;
    }

    //Define event actions
    IEnumerator Zoom() { 
        camRig.ZoomTo(20f, duration: 4); 
        yield return null;
    }

    IEnumerator MoveAndColor() {
        //Wiggle
        ballCreature.MoveTo(new Vector3(2f, 0, -2), duration: 1f);
        yield return new WaitForSeconds(2f);
        ballCreature.MoveTo(new Vector3(-2f, 0, -3), duration: 1f);
        yield return new WaitForSeconds(2f);
        ballCreature.MoveTo(new Vector3(0f, 0, 0), duration: 1f);
        yield return new WaitForSeconds(2f);

        //Walk around and change color
        ballCreature.WalkTo(new Vector3(3, 0, -3), duration: 1);
        yield return new WaitForSeconds(2);
        MeshRenderer mr = ballCreature.transform.Find("body").GetComponent<MeshRenderer>();
        Material mat = mr.material;
        StartCoroutine(ballCreature.changeColor(mat, new Color(1, 0, 0, 1), duration: 0.5f, ease: EaseMode.None));
        ballCreature.WalkTo(new Vector3(-3, 0, 3), duration: 1);
        yield return new WaitForSeconds(2);
        ballCreature.WalkTo(new Vector3(-3, 0, -3), duration: 1);
        yield return new WaitForSeconds(2);
    }

    IEnumerator Disappear() {
        ballCreature.ScaleDownToZero();
        yield return new WaitForSeconds(1);
    }

    //Construct schedule
    protected override void DefineSchedule() {
        /*
        If flexible is true, blocks run as long (or not) as they need to,
        with later blocks waiting until StopWaiting is called. Then,
        the next block starts on the next frame and all later block timings
        are shifted by the same amount.
        Useful for simulations whose duration is not predetermined
        */
        new SceneBlock(0f, Appear);
        new SceneBlock(3f, Zoom);
        new SceneBlock(5f, MoveAndColor, flexible: true);
        new SceneBlock(7f, Disappear);
    }
}
