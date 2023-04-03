using Primer.Animation;
using UnityEngine;
using Shapes;

namespace Primer.Shapes
{
    [RequireComponent(typeof(Line))]
    public class PrimerLine : MonoBehaviour, IPoolable
    {
        private const float DEFAULT_DASH_SIZE = 5f;
        private const float DEFAULT_DASH_SPEED = 0.1f;
        public static readonly IPool<PrimerLine> pool = new PrefabPool<PrimerLine>("PrimerLine");

        private Line lineCache;
        private Line line => lineCache ??= GetComponent<Line>();

        private float dashOffsetMovement = 0f;


        #region Wrap Shapes.Line properties
        public Vector3 start {
            get => line.Start;
            set => line.Start = value;
        }

        public Vector3 end {
            get => line.End;
            set => line.End = value;
        }

        public float thickness {
            get => line.Thickness;
            set => line.Thickness = value;
        }

        public Color color {
            get => line.Color;
            set => line.Color = value;
        }

        public float dashSize {
            get => line.DashSize;
            set => line.DashSize = value;
        }

        public float dashSpacing {
            get => line.DashSpacing;
            set => line.DashSpacing = value;
        }

        public float dashOffset {
            get => line.DashOffset;
            set => line.DashOffset = value;
        }
        #endregion


        public void SetDefaults()
        {
            line.Dashed = false;
            line.DashSize = DEFAULT_DASH_SIZE;
            line.DashSpacing = 0f;
            line.DashOffset = 0f;
            dashOffsetMovement = 0f;
        }

        private void Update()
        {
            if (dashOffsetMovement is not 0)
                line.DashOffset += dashOffsetMovement;
        }


        public void Dashed(float size = DEFAULT_DASH_SIZE)
        {
            line.Dashed = true;
            line.DashSize = size;
        }

        public void DashedAnimated(float speed = DEFAULT_DASH_SPEED, float size = DEFAULT_DASH_SIZE)
        {
            Dashed(size);
            dashOffsetMovement = speed;
        }

        public PrimerLine Set(float? x = null, float? y = null, float? z = null)
        {
            start = new Vector3(x ?? start.x, y ?? start.y, z ?? start.z);
            end = new Vector3(x ?? end.x, y ?? end.y, z ?? end.z);
            return this;
        }

        public Tween MoveTo(float? x = null, float? y = null, float? z = null)
        {
            var fromStart = start;
            var fromEnd = end;
            var toStart = new Vector3(x ?? fromStart.x, y ?? fromStart.y, z ?? fromStart.z);
            var toEnd = new Vector3(x ?? fromEnd.x, y ?? fromEnd.y, z ?? fromEnd.z);

            return new Tween(t => {
                start = Vector3.Lerp(fromStart, toStart, t);
                end = Vector3.Lerp(fromEnd, toEnd, t);
            });
        }

        public void OnReuse()
        {
            SetDefaults();
        }

        public void OnRecycle() {}
    }
}
