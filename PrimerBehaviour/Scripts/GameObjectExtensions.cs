using System.Collections.Generic;
using UnityEngine;
public static class GameObjectExtensions
{
    public static void RemoveEditorGeneratedChildren(this GameObject gameObject) {
#if UNITY_EDITOR
        var toDispose = new List<GameObject>();

        foreach (Transform child in gameObject.transform) {
            // Maybe there is a better way to detect generated objects
            if (child.gameObject.name.EndsWith("(Clone)")) {
                toDispose.Add(child.gameObject);
            }
        }

        foreach (var child in toDispose) {
            child.Dispose();
        }
#endif
    }

    public static void Dispose(this GameObject gameObject) {
        if (Application.isEditor) {
            Object.DestroyImmediate(gameObject);
        }
        else {
            Object.Destroy(gameObject);
        }
    }
}
