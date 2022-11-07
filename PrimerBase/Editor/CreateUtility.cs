using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateUtility
{
    public static void Prefab(string path) {
        var resource = Resources.Load(path);

        if (resource is null) {
            throw new Exception($"Cannot find prefab {path}");
        }

        var newObject = PrefabUtility.InstantiatePrefab(resource) as GameObject;
        Place(newObject);
    }

    public static void Object(string name, params Type[] types) {
        var newObject = ObjectFactory.CreateGameObject(name, types);
        Place(newObject);
    }

    static void Place(GameObject go) {
        var lastView = SceneView.lastActiveSceneView;

        // We bypass this rule because in this case it's either a valid object or a true null
        // ReSharper disable once Unity.NoNullPropagation
        go.transform.position = lastView?.pivot ?? Vector3.zero;

        StageUtility.PlaceGameObjectInCurrentStage(go);
        GameObjectUtility.EnsureUniqueNameForSibling(go);

        Undo.RegisterCreatedObjectUndo(go, $"Create Object: ${go.name}");
        Selection.activeGameObject = go;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
