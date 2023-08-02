using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
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
            var gnome = new Gnome(transform);

            for (var k = 0; k < length.z; k++)
            for (var j = 0; j < length.y; j++)
            for (var i = 0; i < length.x; i++) {
                var coordinates = new Vector3Int(i, j, k);

                var child = CreateCell(gnome, $"Cell {coordinates}");
                child.coordinates = coordinates;

                var position = Vector3.Scale(coordinates, cellSize);
                position.y *= -1;
                child.transform.localPosition = position;
            }
        }

        private Cell CreateCell(Gnome gnome, string cellName)
        {
            if (prefab is null || prefab.isEmpty)
                return gnome.Add<Cell>(cellName);

            var child = gnome.Add(prefab, cellName);
            return child.GetOrAddComponent<Cell>();
        }

        public class Cell : MonoBehaviour
        {
            public Vector3Int coordinates;
            // public Vector3 unitCoordinates;
        }
    }
}
