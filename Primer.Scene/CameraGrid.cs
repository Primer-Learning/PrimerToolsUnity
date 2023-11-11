using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Primer.Scene
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraGrid : MonoBehaviour
    {
        [Min(1)] public int lineWidth = 2;
        
        [Range(0, 1)]
        public float xSpacingFraction = 0.25f;
        [Range(0, 1)]
        public float ySpacingFraction = 0.25f;
        [Range(0, 1)]
        public float xCenterFraction = 0.5f;
        [Range(0, 1)]
        public float yCenterFraction = 0.5f;
        
        // CellWidth here is the width of the line and the space together.
        // In the GridLayout Group, the cell size is just the width of the line.
        // Here, we calculate as if the line is zero thickness, then below we need to correct for the line thickness.
        public int xCellWidth => (int) (canvas.GetComponent<RectTransform>().sizeDelta.x * xSpacingFraction);
        public int yCellWidth => (int) (canvas.GetComponent<RectTransform>().sizeDelta.y * ySpacingFraction);
        public int xDivisions => (int) (canvas.GetComponent<RectTransform>().sizeDelta.x / xCellWidth);
        public int yDivisions => (int) (canvas.GetComponent<RectTransform>().sizeDelta.y / yCellWidth);
        public int xCenter => (int) (canvas.GetComponent<RectTransform>().sizeDelta.x * xCenterFraction);
        public int yCenter => (int)(canvas.GetComponent<RectTransform>().sizeDelta.y * yCenterFraction);
        
        private Canvas canvas;

        private void OnDisable() => canvas.Dispose();

        private void OnValidate()
        {
            if (!enabled) return;

            canvas = GetOrCreate<Canvas>("Canvas", (component, go) => {
                component.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
                go.hideFlags = HideFlags.HideAndDontSave;
            });

            var sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;

            // Draw the grid, correcting spacing and padding for the line thickness
            DrawGridLayout(
                canvas,
                "vGridGroup",
                xDivisions,
                new Vector2(lineWidth, sizeDelta.y),
                new Vector3( xCellWidth - lineWidth, 0),
                new Vector2Int(xCenter % xCellWidth - lineWidth / 2, 0)
            );

            DrawGridLayout(
                canvas,
                "hGridGroup",
                yDivisions,
                new Vector2(sizeDelta.x, lineWidth),
                new Vector3( 0, yCellWidth - lineWidth),
                new Vector2Int(0, yCenter % yCellWidth - lineWidth / 2)
            );
        }

        private static void DrawGridLayout(Component parent, string name, int divisions, 
            Vector2 cellSize, Vector3 spacing, Vector2Int padding
            )
        {
            var group = GetOrCreate<GridLayoutGroup>(name, (component, go) => {
                go.transform.parent = parent.transform;

                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                component.childAlignment = TextAnchor.LowerLeft;
            });

            group.cellSize = cellSize;
            group.spacing = spacing;
            group.padding.left = padding.x;
            group.padding.bottom = padding.y;

            var dividers = group.GetComponentsInChildren<Image>().ToList();

            while (dividers.Count > divisions + 1) {
                dividers[0].gameObject.Dispose(defer: true);
                dividers.RemoveAt(0);
            }
            
            for (var i = dividers.Count; i < divisions + 1; i++) {
                Debug.Log("Adding divider");
                var image = new GameObject().AddComponent<Image>();
                image.transform.SetParent(group.transform);
            }
            
            // Make sure the layout is updated
            LayoutRebuilder.MarkLayoutForRebuild(group.GetComponent<RectTransform>());
        }

        private static T GetOrCreate<T>(string name, Action<T, GameObject> initializer = null) where T : Component
        {
            var child = GameObject.Find(name);

            if (child != null) {
                return child.GetComponent<T>();
            }

            var newChild = new GameObject {name = name};
            var component = newChild.AddComponent<T>();

            if (initializer is not null)
                initializer(component, newChild);

            return component;
        }
    }
}

