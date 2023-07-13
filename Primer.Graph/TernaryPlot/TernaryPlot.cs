using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public class TernaryPlot : MonoBehaviour
    {
        [Button]
        public void Reset()
        {
            DrawBorders();
            Clear();
        }

        public Container GetContentContainer(string id = "Content")
        {
            return transform.FindOrCreate(id).ToContainer();
        }

        public void Clear()
        {
            var container = new Container(transform);
            container.Add("Borders"); // Don't delete the borders
            container.Purge();
        }

        public void DrawBorders()
        {
            if (gameObject.IsPreset())
                return;

            var borders = transform.FindOrCreate("Borders").ToContainer();

            var bottom = borders.AddPrimitive(PrimitiveType.Cylinder, "Bottom line");
            bottom.localPosition = CoordinatesToPosition(0.5f, 0.5f, 0);
            bottom.localScale = new Vector3(0.01f, 0.5f, 0.01f);
            bottom.localRotation = Quaternion.Euler(0, 0, 90);

            var left = borders.AddPrimitive(PrimitiveType.Cylinder, "Left line");
            left.localPosition = CoordinatesToPosition(0.5f, 0, 0.5f);
            left.localScale = new Vector3(0.01f, 0.5f, 0.01f);
            left.localRotation = Quaternion.Euler(0, 0, -30);

            var right = borders.AddPrimitive(PrimitiveType.Cylinder, "Right line");
            right.localPosition = CoordinatesToPosition(0, 0.5f, 0.5f);
            right.localScale = new Vector3(0.01f, 0.5f, 0.01f);
            right.localRotation = Quaternion.Euler(0, 0, 30);

            borders.Purge();
        }

        public Vector3 CoordinatesToPosition(float left, float right, float top)
        {
            var sum = left + right + top;

            if (Mathf.Abs(sum - 1) > 0.00001f)
                Debug.LogWarning($"Sum of coordinates must be 1 but it's {sum}");

            if (left < 0 || right < 0 || top < 0)
                Debug.LogWarning($"Coordinates must all be positive, but they are {left}, {right}, and {top}");

            return new Vector3(
                x: right + top / 2,
                y: top * Mathf.Sqrt(3) / 2,
                z: 0
            );
        }

        public Vector3 CoordinatesToPosition(float[] coords)
        {
            if (coords.Length != 3)
                throw new ArgumentException($"Ternary plot requires 3 coordinates but received {coords.Length}");

            return CoordinatesToPosition(coords[0], coords[1], coords[2]);
        }
    }
}
