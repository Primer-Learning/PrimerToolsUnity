using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

// Change the class name to whatever you want.
// Make sure it matches the file name, or Unity will get real mad
public class PoissonTest : Director
{
    public GameObject container;

    PoissonDiscPointSet pdps;
    List<GameObject> spheres = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }
    //Define event actions
    protected virtual IEnumerator Appear()
    {
        // camRig.transform.position = new Vector3(15, 15, -30);
        pdps = new PoissonDiscPointSet(3, new Vector2(30, 30), overflowMode: PoissonDiscOverflowMode.Force);
        pdps.AddPointsUntilFull();
        int i = 0;
        for (; i < pdps.points.Count; i++)
        {
            //addSphere(centered[i]);
            addSphere(pdps.points[i]);
        }
        for (int j = 0; j < 50; j++)
        {
            pdps.AddPoint();
            addSphere(pdps.points[pdps.points.Count - 1]);
        }
        List<Vector2> centered = pdps.GetCenteredPoints();
        Debug.Log("Number of spheres: " + container.transform.childCount.ToString());
        //yield return new WaitForSeconds(2);
        //pdps.AddPoints(200);
        //for (; i < pdps.points.Count; i++)
        //{
        //    addSphere(pdps.points[i]);
        //}
        //Debug.Log("Number of spheres: " + container.transform.childCount.ToString());
        //Debug.Log("Size of points list: " + pdps.points.Count);
        yield return new WaitForSeconds(5);
    }

    void addSphere(Vector2 point)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(point.x, point.y, 0);
        sphere.transform.parent = container.transform;
        spheres.Add(sphere);
    }

    //Construct schedule
    protected override void DefineSchedule()
    {
        new SceneBlock(1f, Appear);
    }
}
