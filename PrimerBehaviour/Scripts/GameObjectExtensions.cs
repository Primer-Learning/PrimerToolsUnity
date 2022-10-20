using System.Collections.Generic;
using UnityEngine;
public static class GameObjectExtensions
{
    public static string GENERATED_GAME_OBJECT_PREFIX = "*";

    public static void RemoveGeneratedChildren(this GameObject gameObject) {
#if UNITY_EDITOR
        var toDispose = new List<GameObject>();

        foreach (Transform child in gameObject.transform) {
            // Maybe there is a better way to detect generated objects
            if (child.gameObject.name.StartsWith(GENERATED_GAME_OBJECT_PREFIX)) {
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
