using System;
using Primer.Math;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Table
{
    [ExecuteAlways]
    public class GridGenerator : GeneratorBehaviour
    {
        // TODO: Accepts a Vector3 o Func<Vector3> or something that contains a Func<Vector3>
        // public Provider<Vector3> cellSize;

        public Vector2 size = new(3, 3);
        public Vector3 cellSize = new(1, 1, 0);

        [Title("Prefab")]
        public Transform prefab;
        public Vector3 scale = Vector3.one;

        internal CellPlacerEquation placer = new();
        private Func<Vector3, float, float, Vector3> displacer;


        public void DisplaceCells(Func<Vector3, float, float, Vector3> newDisplacer)
        {
            displacer = newDisplacer;
            UpdateChildren();
        }


        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration children)
        {
            var cols = size.x - 1;
            var rows = size.y - 1;

            placer.start = Vector3.zero;
            placer.end = new Vector3(cellSize.x * cols, cellSize.y * rows, 0);

            for (var j = 0; j < size.y; j++) {
                for (var i = 0; i < size.x; i++) {
                    var x = i / cols;
                    var y = j / rows;
                    var pos = placer.Evaluate(x, y);
                    var child = children.Next<PrimerText2>($"Cell {i}-{j}", label => label.fontSize = 2);

                    if (displacer is not null)
                        pos = displacer(pos, x, y);

                    child.text = $"{i + j * size.x}";
                    child.transform.localPosition = new Vector3(pos.x, -pos.y, pos.z);
                }
            }
        }
    }
}
