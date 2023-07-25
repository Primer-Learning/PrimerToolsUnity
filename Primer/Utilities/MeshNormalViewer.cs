using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Primer
{
    // Using game objects to draw the normals is a bit more expensive than using Gizmos.
    // But we don't need to recalculate them whenever the scene is redrawn, so it's actually smoother.
    public class MeshNormalViewer : MonoBehaviour
    {
        private List<GameObject> lines = new List<GameObject>();

        [Button("Draw Normals")]
        public void DrawNormals()
        {
            // Remove existing normal lines.
            ClearNormals();

            // Create a new parent GameObject.
            GameObject parent = new GameObject("Normals Parent");
            parent.transform.parent = transform;

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) return;

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null) return;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector3 vertex = transform.TransformPoint(mesh.vertices[i]);
                Vector3 normal = transform.TransformDirection(mesh.normals[i]);

                // Create a new line GameObject.
                GameObject line = new GameObject("Normal " + i);
                line.transform.parent = parent.transform;
                line.transform.position = vertex;

                // Add a LineRenderer to the line GameObject.
                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, normal);
                lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
                lineRenderer.sharedMaterial.color = Color.green;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;

                // Track the created line.
                lines.Add(line);
            }
        }

        [Button("Clear Normals")]
        public void ClearNormals()
        {
            // Remove existing parent GameObject.
            Transform parentTransform = transform.Find("Normals Parent");
            if (parentTransform != null)
            {
                DestroyImmediate(parentTransform.gameObject);
            }
            lines.Clear();
        }
    }
}