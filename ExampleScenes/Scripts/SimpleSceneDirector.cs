using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSceneDirector : Director
{
    [Header("Scene parameters")]
    [SerializeField] PrimerCharacter blobCreaturePrefab = null;
    [SerializeField] GameObject ground = null;
    PrimerCharacter blobCreature;
    protected override void Awake() {
        base.Awake();
        //Do initialization stuff here
    }

    IEnumerator Appear() {
        /*
        This is an example scene block.
        Technically, it's a coroutine (IEnumerator) that is used 
        to create a scene block in DefineSchedule() below,
        but conceptually, this is where you define 
        a scene block.

        We assign the blobCreature variable by Instantiating the prefab
        This pattern is useful when you want to create a bunch of copies
        of an object, either upfront or dynamically as code runs. We didn't
        really need a prefab here, but I used it as an example.

        blobCreature's parent is the ground game object's Transform component
        The ground object exists in the scene before we hit play (it's 
        called "Plane" in the editor), and it's assigned to the ground variable
        via the editor. It's also possible to find existing game objects via code.
        The pattern of creating game objects in the editor (instead of using 
        prefabs) is useful if you want to manually set the position or other attributes.
        */

        blobCreature = Instantiate(blobCreaturePrefab, ground.transform);
        blobCreature.transform.localPosition = Vector3.zero;
        blobCreature.transform.localRotation = Quaternion.Euler(0, 180, 0);

        // Animate the blob appearing
        blobCreature.transform.localScale = Vector3.zero;
        blobCreature.ScaleUpFromZero();

        yield return new WaitForSeconds(0.5f);
        blobCreature.Wave(duration: 2, smile: true);
    }

    //Define event actions
    IEnumerator Zoom() {
        // This sceneblock is kind of silly because it doesn't need yield statements,
        // but you can still set things up this way to conceptually organize a scene

        camRig.ZoomTo(20f, duration: 4); 
        camRig.RotateTo(Quaternion.Euler(17, 0, 0), duration: 4);
        yield return null;
        // An IEnumerator still needs a yield statement in it, though
    }

    IEnumerator MoveAndColor() {
        // MoveTo examples
        blobCreature.MoveTo(new Vector3(2f, 0, -2), duration: 1f);
        yield return new WaitForSeconds(2f);
        blobCreature.MoveTo(new Vector3(-2f, 0, -3), duration: 1f);
        yield return new WaitForSeconds(2f);
        blobCreature.MoveTo(new Vector3(0f, 0, 0), duration: 1f);
        yield return new WaitForSeconds(2f);

        //Walk around and change color
        blobCreature.WalkTo(new Vector3(3, 0, -3), duration: 1);
        yield return new WaitForSeconds(2);
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
        SceneBlocks start their coroutines at the scene time specified,
        whether or not the previous SceneBlock's coroutine is finished.
        Usually, it's not a great idea to make sceneblocks overlap, but it can
        be useful sometimes, and the system avoids changing timings automatically,
        since the main purpose of the Director is to sync timings to a voice
        recording.

        The one place where the system changes timings automatically is when
        flexible is set to true for a block. In that case, later blocks will wait 
        for it to finish. The next block starts on the next frame and all later 
        block timings are shifted to correct for the difference. 

        For example, the MoveAndColor scene block will start five seconds after the 
        scene begins. The next block is set to start at 7 seconds, but because 
        MoveAndColor has flexible set to true, it will wait until MoveAndColor is
        finished and then start immediately afterward.

        This is useful for simulations whose duration is not predetermined.
        */
        new SceneBlock(0f, Appear);
        new SceneBlock(3f, Zoom);
        new SceneBlock(5f, MoveAndColor, flexible: true);
        new SceneBlock(7f, Disappear); // This block will actually start later than 7 seconds in
    }
}
