using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    [ExecuteAlways]
    public class GraphDomain : MonoBehaviour, IDisposable
    {
        public enum Behaviour
        {
            FollowPoint,
            InvokeMethod,
        }

        [SerializeField]
        [LabelText("Graph")]
        [InfoBox("It will autofill if this object is a child of a Graph component")]
        private Graph graphCache;
        public Graph graph {
            get => transform.ParentComponent(ref graphCache);
            internal set => graphCache = value;
        }

        #region public Behaviour behaviour;
        [SerializeField, HideInInspector]
        public Behaviour _behaviour = Behaviour.FollowPoint;

        [ShowInInspector]
        public Behaviour behaviour {
            get => _behaviour;
            set {
                if (_behaviour == value)
                    return;

                _behaviour = value;
                Update();
            }
        }
        #endregion

        #region public Vector3 point;
        [SerializeField, HideInInspector]
        private Vector3 _point;

        [ShowInInspector]
        [ShowIf("behaviour", Behaviour.FollowPoint)]
        public Vector3 point {
            get => _point;
            set {
                _point = value;
                Update();
            }
        }
        #endregion

        #region public bool useGlobalPosition;
        [SerializeField, HideInInspector]
        private bool _useGlobalPosition;

        [ShowInInspector]
        [ShowIf("behaviour", Behaviour.FollowPoint)]
        public bool useGlobalPosition {
            get => _useGlobalPosition;
            set {
                _useGlobalPosition = value;
                Update();
            }
        }
        #endregion

        [ShowIf("behaviour", Behaviour.InvokeMethod)]
        public Action onDomainChange;


        public Vector3 TransformPoint(Vector3 point)
        {
            return graph.DomainToPosition(point);
        }

        public Vector3 TransformPointToGlobal(Vector3 point)
        {
            return transform.TransformPoint(graph.DomainToPosition(point));
        }

        private void OnDomainChange()
        {
            onDomainChange?.Invoke();
        }

        private void OnEnable()
        {
            graph.onDomainChanged += OnDomainChange;
        }

        private void OnDisable()
        {
            graph.onDomainChanged -= OnDomainChange;
        }

        private void Update()
        {
            if (behaviour != Behaviour.FollowPoint || !graph)
                return;

            if (useGlobalPosition)
                transform.position = graph.DomainToPosition(point);
            else
                transform.localPosition = graph.DomainToPosition(point);
        }

        public void Dispose()
        {
            new Container(transform).Dispose();
        }
    }
}
