using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Table
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class GridGenerator : GeneratorBehaviour
    {
        // TODO: Accepts a Vector3 o Func<Vector3> or something that contains a Func<Vector3>
        // public Provider<Vector3> cellSize;


        [FormerlySerializedAs("size")]
        public Vector3Int length = new(3, 3, 1);
        public Vector3 cellSize = new(1, 1, 0);

        [HideLabel, Title("Prefab")]
        public PrefabProvider prefab;


        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration children)
        {
            // var cols = length.x < 2 ? 1 : length.x - 1;
            // var rows = length.y < 2 ? 1 : length.y - 1;
            // var layers = length.z < 2 ? 1 : length.z - 1;

            for (var k = 0; k < length.z; k++) {
                for (var j = 0; j < length.y; j++) {
                    for (var i = 0; i < length.x; i++) {
                        var coordinates = new Vector3Int(i, j, k);
                        var cellName = $"Cell {coordinates}";

                        void Init(Cell cell)
                        {
                            cell.coordinates = coordinates;
                            // cell.unitCoordinates = new Vector3(i / cols, j / rows, k / layers);
                        }

                        var child = prefab is null || prefab.isEmpty
                            ? children.Next<Cell>(cellName, Init)
                            : children.NextIsInstanceOf(
                                provider: prefab,
                                name: cellName,
                                init: instance => {
                                    var cell = instance.gameObject.AddComponent<Cell>();
                                    Init(cell);
                                    return cell;
                                }
                            );

                        var position = Vector3.Scale(coordinates, cellSize);
                        position.y *= -1;
                        child.transform.localPosition = position;
                    }
                }
            }
        }

        public class Cell : MonoBehaviour
        {
            public Vector3Int coordinates;
            // public Vector3 unitCoordinates;
        }
    }
}
