using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleSceneDirector : Director
{
    [SerializeField] PrimerObject ballBlob = null;
    protected override void Awake() {
        base.Awake();
        //Do initialization stuff here
        camRig.GoToStandardPositions();
        //e.g., set object scale to zero if they're going to scale in
        ballBlob.transform.localScale = Vector3.zero;
    }

    //Called each frame
    protected override void Update() {
        base.Update();
        float rotSpeed = 4; //degrees per second
        camRig.transform.localRotation = Quaternion.Euler(0, rotSpeed * Time.deltaTime, 0) * camRig.transform.localRotation;
    }
    
    IEnumerator Appear() {
        ballBlob.ScaleUpFromZero();
        yield return null;
    }

    //Define event actions
    IEnumerator Zoom() { 
        camRig.ZoomTo(20f, duration: 4); 
        yield return new WaitForSeconds(4);
    }

    IEnumerator MoveAndColor() {
        //Wiggle
        ballBlob.MoveTo(new Vector3(2f, 0.5f, -2), duration: 1f);
        yield return new WaitForSeconds(2f);
        ballBlob.MoveTo(new Vector3(-2f, 0.5f, -3), duration: 1f);
        yield return new WaitForSeconds(2f);
        ballBlob.MoveTo(new Vector3(0f, 0.5f, 0), duration: 1f);
        yield return new WaitForSeconds(2f);

        //Walk around and change color
        ballBlob.WalkTo(new Vector3(3, 0.5f, -3), duration: 1);
        yield return new WaitForSeconds(2);
        MeshRenderer mr = ballBlob.GetComponent<MeshRenderer>();
        Material mat = mr.material;
        StartCoroutine(ballBlob.changeColor(mat, new Color(1, 0, 0, 1), duration: 0.5f, ease: EaseMode.None));
        //ballBlob.ChangeColor(PrimerConstants.GetColor("Red"));
        ballBlob.WalkTo(new Vector3(-3, 0.5f, 3), duration: 1);
        yield return new WaitForSeconds(2);
        ballBlob.WalkTo(new Vector3(-3, 0.5f, -3), duration: 1);
        yield return null;
    }

    IEnumerator Disappear() {
        ballBlob.ScaleDownToZero();
        yield return null;
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
        new SceneBlock(5f, MoveAndColor);
        new SceneBlock(17f, Disappear);
    }
}
