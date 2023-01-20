using UnityEngine;

namespace Primer.Scene
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraRig : MonoBehaviour
    {
        private Camera cameraCache;
        internal Camera cam => cameraCache == null ? cameraCache = GetComponent<Camera>() : cameraCache;


        public float distance = 10;
        public Vector3 swivelOrigin;
        public Vector3 swivel;
        public bool faceSwivel = true;
        public Color backgroundColor = new(41 / 255f, 45 / 255f, 47 / 255f);


        public float ActualDistance => (transform.position - swivelOrigin).magnitude;


        private void Update() => Validate();
        private void OnValidate() => Validate();
        private void OnDrawGizmos() => Gizmos.DrawSphere(swivelOrigin, 0.1f);


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
    }
}

