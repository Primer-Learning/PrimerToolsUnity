using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ReferenceScreen : PrimerObject 
{   
    public VideoPlayer screen = null;

    public void Play() { screen.Play(); }
}
