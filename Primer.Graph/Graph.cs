using System;
using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    [ExecuteAlways]
    [RequireComponent(typeof(MultipleAxesController))]
    public class Graph : MonoBehaviour, IDisposable, IAxisController
    {
        [Title("Graph controls")]
        [FormerlySerializedAs("_enableZAxis")]
        public bool enableZAxis;

        [EnableIf(nameof(enableZAxis))]
        [OnValueChanged(nameof(EnsureDomainDimensions))]
        public bool isRightHanded = true;

        private MultipleAxesController axesController;

        [Title("Axes references")]
        [FormerlySerializedAs("x")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis xAxis;

        [FormerlySerializedAs("y")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis yAxis;

        [FormerlySerializedAs("z")]
        [SerializeField]
        [Required]
        [InlineEditor]
        [ChildGameObjectsOnly]
        private Axis zAxis;

        public Axis x => xAxis != null ? xAxis : null;
        public Axis y => yAxis != null ? yAxis : null;
        public Axis z => zAxis != null ? zAxis : null;

        [FormerlySerializedAs("containerCache")]
        public Gnome<Graph> gnomeCache;
        public Gnome<Graph> gnome => gnomeCache ??= InitializeGnome();

        public Vector3 domain { get; private set; }

        public Action onDomainChanged;

        public float scale {
            set {
                x.scale = value;
                y.scale = value;
                z.scale = value;
            }
        }

        public Axis[] axes => enableZAxis && z?.transform.localScale.z is not 0
            ? new [] { x, y, z }
            : new [] { x, y };


        private void OnEnable() => UpdateAxes();

        private void OnValidate() => UpdateAxes();


        public Tween GrowZAxis(float newMax) => GrowZAxis(0, newMax);
        public Tween GrowZAxis(float newMin, float newMax)
        {
            enableZAxis = true;
            return z.GrowFromOrigin(newMin, newMax);
        }

        public Tween ShrinkZAxis()
        {
            return z.ShrinkToOrigin().Observe(onComplete: () => enableZAxis = false);
        }

        public Tween Transition()
        {
            return axes.Select(axis => axis.Transition()).RunInParallel();
        }

        private void UpdateAxes()
        {
            if (zAxis != null) {
                zAxis.SetActive(enableZAxis);

                if (enableZAxis) {
                    zAxis.transform.rotation = isRightHanded
                        ? Quaternion.Euler(0, 90, 0)
                        : Quaternion.Euler(0, -90, 0);
                }
            }

            axesController ??= GetComponent<MultipleAxesController>();
            axesController.SetAxes(EnsureDomainDimensions, xAxis, yAxis, zAxis);
        }

        public Vector3 DomainToPosition(Vector3 value)
        {
            return new Vector3(
                x?.DomainToPosition(value.x) ?? 0,
                y?.DomainToPosition(value.y) ?? 0,
                z?.DomainToPosition(value.z) ?? 0
            );
        }

        private void EnsureDomainDimensions()
        {
            var scale = new Vector3(
                x?.DomainToPosition(1) ?? 1,
                y?.DomainToPosition(1) ?? 1,
                z?.DomainToPosition(1) ?? 1
            );

            if (scale == domain)
                return;

            domain = scale;
            onDomainChanged?.Invoke();
        }

        public PrimerLine AddLine(string name)
        {
            return gnome.Add<PrimerLine>(name);
        }

        public PrimerSurface AddSurface(string name)
        {
            return gnome.Add<PrimerSurface>(name);
        }

        public StackedArea AddStackedArea(string name)
        {
            return gnome.Add<StackedArea>(name);
        }

        public NewBarPlot AddBarPlot(string name)
        {
            return gnome.Add<NewBarPlot>(name);
        }

        public GraphDomain AddPoint(string name, Vector3? coords = null)
        {
            var point = gnome.AddPrimitive(PrimitiveType.Sphere, name);
            point.SetScale(0.1f);
            return Track(point, coords);
        }

        public T AddPoint<T>(string name, T template, Vector3? coords = null) where T : Component
        {
            var point = gnome.Add(template, name);
            Track(point, coords);
            return point;
        }

        public GraphDomain AddTracker<T>(string name, T template, Vector3? coords = null) where T : Component
        {
            var point = gnome.Add(template, name);
            return Track(point, coords);
        }

        public GraphDomain Track(Component target, Vector3? coords = null)
        {
            var tracker = target.GetOrAddComponent<GraphDomain>();
            tracker.graph = this;
            tracker.behaviour = GraphDomain.Behaviour.FollowPoint;

            if (coords is not null)
                tracker.point = coords.Value;

            return tracker;
        }

        public void Reset()
        {
            gnomeCache = InitializeGnome();
        }

        public void Dispose()
        {
            Reset();
            gameObject.SetActive(false);
        }

        private Gnome<Graph> InitializeGnome()
        {
            var result = Gnome.For(this);
            result.Insert(xAxis);
            result.Insert(yAxis);
            result.Insert(zAxis);
            return result;
        }
    }
}
