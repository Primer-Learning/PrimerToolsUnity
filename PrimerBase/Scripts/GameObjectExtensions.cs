using System.Collections.Generic;
using UnityEngine;
public static class GameObjectExtensions
{
    public static void Dispose(this GameObject gameObject) {
        if (Application.isEditor) {
            Object.DestroyImmediate(gameObject);
        }
        else {
            Object.Destroy(gameObject);
        }
    }
}
