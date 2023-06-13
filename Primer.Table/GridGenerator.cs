using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Table
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class GridGenerator : MonoBehaviour
    {
        public Vector3Int length = new(3, 3, 1);
        public Vector3 cellSize = new(1, 1, 0);

        [HideLabel, Title("Prefab")]
        public PrefabProvider prefab;


        [Button(ButtonSizes.Large)]
        public void UpdateChildren()
        {
            var container = new Container(transform);

            for (var k = 0; k < length.z; k++)
            for (var j = 0; j < length.y; j++)
            for (var i = 0; i < length.x; i++) {
                var coordinates = new Vector3Int(i, j, k);

                var child = CreateCell(container, $"Cell {coordinates}");
                child.coordinates = coordinates;

                var position = Vector3.Scale(coordinates, cellSize);
                position.y *= -1;
                child.transform.localPosition = position;
            }
        }

        private Cell CreateCell(Container container, string cellName)
        {
            if (prefab is null || prefab.isEmpty)
                return container.Add<Cell>(cellName);

            var child = container.Add(prefab, cellName);
            return child.GetOrAddComponent<Cell>();
        }

        public class Cell : MonoBehaviour
        {
            public Vector3Int coordinates;
            // public Vector3 unitCoordinates;
        }
    }
}
