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
        [Min(0)] public int xDivisions = 3;
        [Min(0)] public int yDivisions = 3;
        [Min(1)] public int lineWidth = 5;

        private void OnDisable() => GameObject.Find("Canvas")?.Dispose();


        private void OnDrawGizmos()
        {
            if (!enabled) return;

            var canvas = GetOrCreateChild<Canvas>("Canvas", (component, go) => {
                component.renderMode = RenderMode.ScreenSpaceOverlay;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            });

            var sizeDelta = canvas.GetComponent<RectTransform>().sizeDelta;

            DrawGridLayout(
                canvas,
                "vGridGroup",
                xDivisions,
                GridLayoutGroup.Constraint.FixedRowCount,
                new Vector2(lineWidth, sizeDelta.y),
                new Vector3((sizeDelta.x - lineWidth) / xDivisions - lineWidth, 0)
            );

            DrawGridLayout(
                canvas,
                "hGridGroup",
                yDivisions,
                GridLayoutGroup.Constraint.FixedColumnCount,
                new Vector2(sizeDelta.x, lineWidth),
                new Vector3( 0, (sizeDelta.y - lineWidth) / yDivisions - lineWidth)
            );
        }

        private static void DrawGridLayout(Component parent, string name, int divisions,
            GridLayoutGroup.Constraint constraint, Vector2 cellSize, Vector3 spacing)
        {
            var group = GetOrCreateChild<GridLayoutGroup>(name, (component, go) => {
                go.transform.parent = parent.transform;

                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                component.childAlignment = TextAnchor.MiddleCenter;
                component.constraint = constraint;
                component.constraintCount = 1;
            });

            group.cellSize = cellSize;
            group.spacing = spacing;

            var dividers = group.GetComponentsInChildren<Image>().ToList();

            while (dividers.Count > divisions + 1) {
                dividers[0].gameObject.Dispose();
                dividers.RemoveAt(0);
            }

            for (var i = dividers.Count; i < divisions + 1; i++) {
                var image = new GameObject().AddComponent<Image>();
                // image.gameObject.AddComponent<CanvasRenderer>();
                image.transform.parent = group.transform;
            }
        }

        private static T GetOrCreateChild<T>(string name, Action<T, GameObject> initializer = null) where T : Component
        {
            var child = GameObject.Find(name);
            T component;

            if (child == null) {
                child = new GameObject { name = name };
                component = child.AddComponent<T>();

                if (initializer is not null)
                    initializer(component, child);
            }
            else {
                component = child.GetComponent<T>();
            }

            return component;
        }
    }
}

