using System;
using Primer.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Graph.Editor
{
    public static class GraphCreateUtility
    {
        [MenuItem("GameObject/Primer/Graph", false, CreateUtility.PRIORITY)]
        public static void Graph() => CreateUtility.Prefab("Graph2");

        [MenuItem("GameObject/Primer/Graph Points", true)]
        [MenuItem("GameObject/Primer/Graph Line", true)]
        [MenuItem("GameObject/Primer/Graph Surface", true)]
        public static bool ValidateIsGraph(MenuCommand command) =>
            // Selection can only be null or a live game object in the hierarchy panel
            // it can never be false null, so we disable this rule on this file
            // ReSharper disable once Unity.NoNullPropagation
            Selection.activeGameObject?.GetComponent<Graph2>() is not null;

        [MenuItem("GameObject/Primer/Graph Points", false, CreateUtility.PRIORITY)]
        public static void Points(MenuCommand command) {
            var graph = GetGraph(command);
            var point = CreateUtility.Prefab("GraphPoints", graph.domain);
            point.transform.localPosition = Vector3.zero;
        }

        [MenuItem("GameObject/Primer/Graph Line", false, CreateUtility.PRIORITY)]
        public static void Line(MenuCommand command) {
            var graph = GetGraph(command);
            var line = CreateUtility.Prefab("GraphLine", graph.domain);
            line.transform.localPosition = Vector3.zero;
        }

        [MenuItem("GameObject/Primer/Graph Surface", false, CreateUtility.PRIORITY)]
        public static void Surface(MenuCommand command) {
            var graph = GetGraph(command);
            var line = CreateUtility.Prefab("GraphSurface", graph.domain);
            line.transform.localPosition = Vector3.zero;
        }

        static Graph2 GetGraph(MenuCommand command) {
            if (command.context is not GameObject go) {
                throw new Exception("This should never happen");
            }

            var graph = go.GetComponent<Graph2>();

            if (graph is null) {
                throw new Exception("Create graph surface command should only be executed on a graph");
            }

            return graph;
        }
    }
}
