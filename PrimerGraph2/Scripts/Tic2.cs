using System;
using UnityEngine;
public class Tic2 : PrimerBehaviour
{
    public float value;
    PrimerText text;

    public void Initialize(PrimerText primerTextPrefab, TicData data, float distance) {
        if (text != null) {
            throw new Exception($"Tic {value} has already been initialized");
        }

        text = Instantiate(primerTextPrefab, transform);
        text.transform.SetLocalPositionAndRotation(
            new Vector3(0f, -distance, 0f),
            Quaternion.Inverse(transform.parent.rotation)
        );

        value = data.value;
        text.tmpro.text = data.label;
    }
}


[Serializable]
public class TicData
{
    public float value;
    public string label;

    public TicData(float value, string label) {
        this.value = value;
        this.label = label;
    }
}
