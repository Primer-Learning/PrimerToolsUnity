using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

// Change the class name to whatever you want.
// Make sure it matches the file name, or Unity will get real mad
public class DirectorTemplate : Director
{
    protected override void Awake() {
        base.Awake();
    }
    protected override void Start() {
        base.Start();
    }

    //Define event actions
    protected virtual IEnumerator Appear() {
        // This is an example scene block.
        // Technically, it's a coroutine that is used 
        // to create a scene block in DefineSchedule(),
        // but conceptually, this is where you define 
        // a scene block.

        // Couroutines need a yield statement.
        // Otherwise, they should just be regular functions.
        yield return null;
    }
    protected virtual IEnumerator DoStuff() {
        // You can make as many of these as you like
        yield return null;
    }
    
    //Construct schedule
    protected override void DefineSchedule() {
        // Here's where you actually create the scene blocks and 
        // specify their timing.
        // You can put a scene time in seconds (float) 
        // or in minutes and seconds (int, float) 
        new SceneBlock(1f, Appear);
        new SceneBlock(1, 1f, DoStuff);
    }
}
