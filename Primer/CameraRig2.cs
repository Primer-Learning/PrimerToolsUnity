using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Primer
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraRig2 : MonoBehaviour
    {
        private Camera cameraCache;
        internal Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;


        public float distance = 10;
        public Vector3 swivelOrigin;
        public Vector3 swivel;
        public bool faceSwivel = true;
        public Color backgroundColor = new(41 / 255f, 45 / 255f, 47 / 255f);
        [Space]
        public bool gridOverlay;
        [Min(0)] public int xDivisions = 3;
        [Min(0)] public int yDivisions = 3;
        [Min(1)] public int lineWidth = 5;


        public float ActualDistance => (transform.position - swivelOrigin).magnitude;


        private void Update() => Validate();
        private void OnValidate() => Validate();


        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(swivelOrigin, 0.1f);
            DrawGridOverlay();
        }


        private Vector3? oldSwivelOrigin;
        private Vector3? oldSwivel;
        private bool? lastFaceSwivel;
        private void Validate()
        {
            var isDistanceWrong = Mathf.Abs(ActualDistance - distance) > 0.1;
            var isPositionOutdated = lastFaceSwivel != faceSwivel || oldSwivelOrigin != swivelOrigin || isDistanceWrong;

            if (isPositionOutdated || oldSwivel != swivel) {
                UpdateSwivel();
                lastFaceSwivel = faceSwivel;
                oldSwivelOrigin = swivelOrigin;
                oldSwivel = swivel;
            }

            if (cam != null && backgroundColor != cam.backgroundColor) {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
            }
        }

        private void UpdateSwivel()
        {
            var transform = this.transform;

            var direction = faceSwivel ? Vector3.back : Vector3.forward;
            transform.position = Quaternion.Euler(swivel) * direction * distance + swivelOrigin;

            transform.LookAt(swivelOrigin);

            // handle rotation in the axis the camera is pointing at as LookAt can't do this
            if (swivel.z != 0) {
                transform.Rotate(0, 0, swivel.z);
            }
        }


        #region Grid rendering
        private void DrawGridOverlay()
        {
            if (!gridOverlay) {
                GameObject.Find("Canvas")?.Dispose();
                return;
            }

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
        #endregion
    }
}

