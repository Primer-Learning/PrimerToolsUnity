using System.Collections.Generic;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Scene
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        private Camera cameraCache;
        public Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;

        [SerializeField, HideInInspector]
        private float _distance = 10;
        [ShowInInspector]
        public float distance {
            get => _distance;
            set {
                
                _distance = value;
                UpdateSwivel();
            }
        }
        
        [SerializeField, HideInInspector]
        private Vector3 _swivelOrigin;
        [ShowInInspector]
        public Vector3 swivelOrigin {
            get => _swivelOrigin;
            set
            {
                _swivelOrigin = value;
                UpdateSwivel();
            }
        }
        
        [SerializeField, HideInInspector]
        private Vector3 _swivel;
        [ShowInInspector]
        public Vector3 swivel {
            get => _swivel;
            set
            {
                _swivel = value;
                UpdateSwivel();
            }
        }
        
        public bool faceSwivel = true;
        public Color backgroundColor = PrimerColor.gray;

        private void OnDrawGizmos() => Gizmos.DrawSphere(swivelOrigin, 0.1f);

        private void Awake()
        {
            if (cam != null && backgroundColor != cam.backgroundColor) {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = backgroundColor;
            }
        }

        private void UpdateSwivel()
        {
            // var direction = faceSwivel ? Vector3.back : Vector3.forward;
            transform.position = Quaternion.Euler(swivel) * Vector3.back * distance + swivelOrigin;
            transform.rotation = Quaternion.Euler(swivel);
        }

        public Tween FocusOn(Component target, Vector3 offset, float? distance = null, Vector3? swivel = null)
        {
            return Travel(distance, target.transform.position + offset, swivel);
        }

        public Tween Travel(float? distance = null, Vector3? swivelOrigin = null, Vector3? swivel = null)
        {
            var tween = new List<Tween>();
            var linear = LinearEasing.instance;

            if (distance.HasValue)
            {
                tween.Add(Tween.Value(
                        v => this.distance = v,
                        () => this.distance,
                        () => distance.Value
                    ) with
                    {
                        easing = linear
                    });
            }

            if (swivelOrigin.HasValue) {
                {
                    tween.Add(Tween.Value(
                            v => this.swivelOrigin = v,
                            () => this.swivelOrigin,
                            () => swivelOrigin.Value
                        ) with
                        {
                            easing = linear
                        });
                }
            }

            if (swivel.HasValue) {
                {
                    tween.Add(Tween.Value(
                            v => this.swivel = v,
                            () => this.swivel,
                            () => swivel.Value
                        ) with
                        {
                            easing = linear
                        });
                }
            }

            // or use tween.RunInBatch() to merge all tweens into one with unified easing
            return tween.RunInParallel() with { easing = IEasing.defaultMethod };
        }

        [PropertySpace]
        [Button(ButtonSizes.Large)]
        private void CopyCode()
        {
            GUIUtility.systemCopyBuffer = $@"
.Travel(
    distance: {distance}f,
    swivelOrigin: {swivelOrigin.ToCode()},
    swivel: {swivel.ToCode()}
)
            ".Trim();
        }
    }
}

