using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

public class CoinFlipScene : Director
{
    [Header("Scene parameters")]
    [SerializeField] CoinFlipSimManager flipperManager = null;
    [SerializeField] PrimerCharacter blobPrefab = null;
    [SerializeField] GameObject blobCoin = null;

    protected override void Awake() {
        base.Awake();
    }
    protected override void Start() {
        // When designing the coins, I set gravity to 2x for some reason (the reason is laziness!)
        Physics.gravity = new Vector3(0, -9.81f * 2, 0);

        camRig.cam.transform.localPosition = new Vector3(0, 0, -7.5f);
        camRig.transform.localPosition = new Vector3(0, 1, 0);
        camRig.transform.localRotation = Quaternion.Euler(17, 0, 0);

        flipperManager.Initialize();
        base.Start();
    }

    //Define event actions
    IEnumerator Appear() {
        // Show the first flipper
        flipperManager.AddFlipper();
        flipperManager.ShowFlippers();
        yield return null;
    }
    IEnumerator MoreFlippers() {
        int totalFlippers = 25;

        // Add the other flippers
        CoinFlipper toSkip = flipperManager.flippers[0];
        flipperManager.AddFlippers(totalFlippers - 1);
        flipperManager.flippers.Remove(toSkip);
        flipperManager.flippers.Insert(0, toSkip);
        flipperManager.ArrangeAsGrid(5, 5, duration: 1f);
        camRig.ZoomTo(40);
        camRig.MoveTo(Vector3.zero);
        camRig.RotateTo(Quaternion.Euler(40, 0, 0));
        yield return new WaitForSeconds(1);
        flipperManager.flippers[12].flipperCharacterPrefab = blobPrefab;
        flipperManager.flippers[12].coinPrefab = blobCoin;
        flipperManager.ShowFlippers(skip: 1);
        yield return new WaitForSeconds(1);
        
        // Do the flips
        // This actually tries to judge whether they are cheaters, but that's
        // irrelevant in this scene.
        yield return flipperManager.testFlippers(24, 18);
        yield return new WaitForSeconds(1);
        flipperManager.WrapUp();
    }

    IEnumerator disappear() {
        foreach (CoinFlipper flipper in flipperManager.flippers) {
        flipper.display.transform.parent = null;
            foreach (PrimerObject coin in flipper.displayCoins) {
                coin.Disappear();
            }
            flipper.flipperCharacter.Disappear();
            flipper.floor.Disappear();
        }
        yield return null;
    }
    
    //Construct schedule
    protected override void DefineSchedule() {
        new SceneBlock(0, Appear);
        new SceneBlock(3, MoreFlippers, flexible: true);
        new SceneBlock(4, disappear);
    }
}
