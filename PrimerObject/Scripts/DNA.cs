using System;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Sirenix.Utilities;
using UnityEngine;

public class DNA : MonoBehaviour
{
    [SerializeField] GameObject[] strandPrefabs = new GameObject[2];
    public Transform[] strands = new Transform[2];
    public Color? color = null;
    private SimpleGnome gnome => new (transform);
    
    public Tween GenerateStrands(float duration = 1, float durationPerPiece = 0.1f) {
        var tweens = new List<Tween>();
        for (var i = 0; i < strands.Length; i++)
        {
            if (strands[i] == null) {
                strands[i] = gnome.Add<Transform>(strandPrefabs[i], $"strand {i}");
                var strandChildren = strands[i].GetComponentsInChildren<Transform>();
                var strandChildrenScales = strandChildren.Select(x => x.localScale).ToArray();
                if (color != null) {
                    strandChildren.ForEach(x => x.GetComponent<Renderer>().SetColor((Color)color));
                }
                strands[i].parent = transform;
                strands[i].localPosition = Vector3.zero;
                strands[i].localRotation = Quaternion.identity;
                
                strandChildren.ForEach(x => x.localScale = Vector3.zero);
                tweens.Add(strandChildren.Select((x, j) => x.ScaleTo(strandChildrenScales[j])).RunInParallel(duration, durationPerPiece));
                
                // tweens.Add(strandChildren.Select(x => x.ScaleUpFromZero()).RunInParallel(duration, durationPerPiece));
            }
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
