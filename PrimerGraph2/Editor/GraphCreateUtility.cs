using System;
using UnityEditor;
using UnityEngine;

public static class GraphCreateUtility
{
    [MenuItem("GameObject/Primer/Graph", false, CreateUtility.PRIORITY)]
    public static void Graph() => CreateUtility.Prefab("Graph2");

    [MenuItem("GameObject/Primer/GraphLine", true)]
    public static bool ValidateLine(MenuCommand command) =>
        // Selection can only be null or a live game object in the hierarchy panel
        // it can never be false null
        // ReSharper disable once Unity.NoNullPropagation
        Selection.activeGameObject?.GetComponent<Graph2>() is not null;

    [MenuItem("GameObject/Primer/GraphLine", false, CreateUtility.PRIORITY)]
    public static void Line(MenuCommand command) {
        if (command.context is not GameObject go) {
            throw new Exception("This should never happen");
        }

        var graph = go.GetComponent<Graph2>();

        if (graph is null) {
            throw new Exception("Create graph line command should only be executed on a graph");
        }

        var line = CreateUtility.Prefab("GraphLine", graph.domain);

        line.transform.localPosition = Vector3.zero;
    }
}
