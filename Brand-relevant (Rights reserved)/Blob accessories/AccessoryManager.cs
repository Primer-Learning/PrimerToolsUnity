using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PrimerBlob))]
public class AccessoryManager : MonoBehaviour
{
    public AccessoryType accessoryType;
    public bool colorMatch;
    private PrimerBlob Blob => GetComponent<PrimerBlob>();

    private void OnValidate()
    {
        Blob.DestoyAccessories();
        Blob.AddAccessory(accessoryType, colorMatch: colorMatch);
    }
}
