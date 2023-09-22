using System;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Sirenix.Utilities;
using UnityEngine;

public class DNA : MonoBehaviour
{
    public float secondsPerRotation = 10;
    [SerializeField] GameObject[] strandPrefabs = new GameObject[2];
    public Transform[] strands = new Transform[2];
    public Color? color = null;
    private SimpleGnome gnome => new (transform);

    public void Update()
    {
        if (secondsPerRotation > 0) transform.Rotate(transform.up, 360 * Time.deltaTime / secondsPerRotation);
    }

    public Tween GenerateStrands(float duration = 1, float durationPerPiece = 0.1f) {
        var tweens = new List<Tween>();
        for (var i = 0; i < strands.Length; i++)
        {
            if (strands[i] == null) {
                strands[i] = gnome.Add<Transform>(strandPrefabs[i], $"strand {i}");
                var strandChildren = strands[i].GetComponentsInChildren<Renderer>();
                var strandChildrenScales = strandChildren.Select(x => x.transform.localScale).ToArray();
                if (color != null) {
                    strandChildren.ForEach(x => x.GetComponent<Renderer>().SetColor((Color)color));
                }
                strands[i].parent = transform;
                strands[i].localPosition = Vector3.zero;
                strands[i].localRotation = Quaternion.identity;
                
                strandChildren.ForEach(x => x.transform.localScale = Vector3.zero);
                tweens.Add(strandChildren.Select((x, j) => x.ScaleTo(strandChildrenScales[j])).RunInParallel(duration, durationPerPiece));
                
                // tweens.Add(strandChildren.Select(x => x.ScaleUpFromZero()).RunInParallel(duration, durationPerPiece));
            }
        }
        return tweens.RunInParallel();
    }

    public Tween TweenColor(Color newColor)
    {
        var tweens = new List<Tween>();
        foreach (var strand in strands)
        {
            if (strand == null) continue;
            var strandChildren = strand.GetComponentsInChildren<Renderer>();
            tweens.Add(strandChildren.Select(x => x.TweenColor(newColor)).RunInParallel());
        }
        return tweens.RunInParallel();
    }

    public void ClearStrands()
    {
        foreach (var strand in strands)
        {
            if (strand != null) {
                strand.Dispose();
            }
        }
    }
    public Transform ReleaseStrand(int index) {
        Transform strand = strands[index];
        strands[index] = null;
        return strand;
    }
    public Tween AcceptStrand(Transform strand, int index) {
        if (strands[index] != null) {
            Debug.LogError($"Strand {index} already exists");
        }
        
        strands[index] = strand;
        strands[index].parent = transform;
        var tweens = new Tween[]
        {
            strands[index].MoveTo(Vector3.zero),
            strands[index].RotateTo(Quaternion.identity),
            strands[index].ScaleTo(Vector3.one)
        };
        return tweens.RunInParallel();
    }
}
