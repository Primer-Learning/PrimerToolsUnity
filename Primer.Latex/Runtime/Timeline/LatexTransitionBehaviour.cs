using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace LatexRenderer.Timeline
{
    public class LatexTransitionBehaviour : PlayableBehaviour
    {
        private Target _afterTarget;
        private float _morphDuration = 0.5f;
        private List<(MorphTarget beforeChild, MorphTarget afterChild)> _morphTargets = new();

        private List<(MorphTarget beforeChild, MorphTarget afterChild)> _scaleDownAndMoveTargets =
            new();

        private List<Target> _scaleDownTargets = new();
        private List<Target> _scaleUpTargets = new();

        public Transform After;
        public Transform AfterAnchor;
        public Transform Before;
        public Transform BeforeAnchor;
        public List<(Transform beforeChild, Transform afterChild)> MorphTransitions;
        public List<(Transform beforeChild, Transform afterChild)> ScaleDownAndMoveTransitions;

        public float MorphDuration
        {
            get => _morphDuration;
            set
            {
                if (value is < 0f or > 1f)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Must be time ratio between 0 and 1 inclusive.");

                _morphDuration = value;
            }
        }

        private void UpdateScaleDowns(float timeRatio)
        {
            var morphTimeRatio = timeRatio / _morphDuration;
            foreach (var i in _scaleDownTargets)
            {
                i.transform.localScale = Vector3.Lerp(i.originalScale, Vector3.zero,
                    morphTimeRatio);
                i.transform.gameObject.SetActive(morphTimeRatio < 1);
            }
        }

        private void UpdateScaleDownAndMoves(float timeRatio)
        {
            var morphTimeRatio = timeRatio / _morphDuration;
            var scaleUpTimeRatio = (timeRatio - _morphDuration) / (1 - _morphDuration);
            foreach (var (beforeChild, afterChild) in _scaleDownAndMoveTargets)
            {
                beforeChild.transform.localScale = Vector3.Lerp(beforeChild.originalScale,
                    Vector3.zero, morphTimeRatio);
                beforeChild.transform.position = Vector3.Lerp(beforeChild.originalWorldPosition,
                    afterChild.transform.position, morphTimeRatio);
                beforeChild.transform.gameObject.SetActive(morphTimeRatio < 1);

                afterChild.transform.localScale = Vector3.Lerp(Vector3.zero,
                    afterChild.originalScale, scaleUpTimeRatio);
                afterChild.transform.gameObject.SetActive(scaleUpTimeRatio > 0);
            }
        }

        private void UpdateScaleUps(float timeRatio)
        {
            var scaleUpTimeRatio = (timeRatio - _morphDuration) / (1 - _morphDuration);
            foreach (var i in _scaleUpTargets)
            {
                i.transform.localScale = Vector3.Lerp(Vector3.zero, i.originalScale,
                    scaleUpTimeRatio);
                i.transform.gameObject.SetActive(scaleUpTimeRatio > 0);
            }
        }

        private void UpdateMorphs(float timeRatio)
        {
            var morphTimeRatio = timeRatio / _morphDuration;
            foreach (var (beforeChild, afterChild) in _morphTargets)
            {
                beforeChild.transform.position = Vector3.Lerp(beforeChild.originalWorldPosition,
                    afterChild.transform.position, morphTimeRatio);

                // NOTE: I'm unsure if this will hold up to a tree of scaling transforms...
                var goalLocalScale = beforeChild.originalScale *
                                     (afterChild.originalSuperBounds.size.x /
                                      beforeChild.originalSuperBounds.size.x);
                beforeChild.transform.localScale = Vector3.Lerp(beforeChild.originalScale,
                    goalLocalScale, morphTimeRatio);

                beforeChild.transform.gameObject.SetActive(morphTimeRatio < 1);
                afterChild.transform.gameObject.SetActive(morphTimeRatio >= 1);
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var timeRatio = (float)(playable.GetTime() / playable.GetDuration());
            UpdateScaleDowns(timeRatio);
            UpdateScaleUps(timeRatio);
            UpdateMorphs(timeRatio);
            UpdateScaleDownAndMoves(timeRatio);
        }

        private static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }

        private void ResetTargets()
        {
            _afterTarget = null;
            _morphTargets = new List<(MorphTarget beforeChild, MorphTarget afterChild)>();
            _scaleDownAndMoveTargets =
                new List<(MorphTarget beforeChild, MorphTarget afterChild)>();
            _scaleDownTargets = new List<Target>();
            _scaleUpTargets = new List<Target>();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var allTargets = Concatenate(
                _morphTargets.SelectMany(i => new[] { i.beforeChild, i.afterChild }),
                _scaleDownAndMoveTargets.SelectMany(i => new[] { i.beforeChild, i.afterChild }),
                _scaleDownTargets, _scaleUpTargets, new[] { _afterTarget });
            foreach (var target in allTargets) target.ApplyOriginalValues();

            ResetTargets();
        }

        private void AlignAnchors()
        {
            var beforeBounds = SuperBounds.GetSuperBounds(BeforeAnchor);
            var afterBounds = SuperBounds.GetSuperBounds(AfterAnchor);

            _afterTarget.transform.position += beforeBounds.center - afterBounds.center;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            ResetTargets();

            _afterTarget = new Target(After);
            AlignAnchors();

            _morphTargets = MorphTransitions.Select(i =>
            {
                if (i.beforeChild.parent != Before)
                    throw new Exception("BeforeChild of morph transition is not child of Before.");

                if (i.afterChild.parent != After)
                    throw new Exception("AfterChild of morph transition is not child of After.");

                return (new MorphTarget(i.beforeChild), new MorphTarget(i.afterChild));
            }).ToList();

            _scaleDownAndMoveTargets = ScaleDownAndMoveTransitions.Select(i =>
            {
                if (i.beforeChild.parent != Before)
                    throw new Exception(
                        "BeforeChild of scale-down-and-move transition is not child of Before.");

                if (i.afterChild.parent != After)
                    throw new Exception(
                        "AfterChild of scale-down-and-move transition is not child of After.");

                return (new MorphTarget(i.beforeChild), new MorphTarget(i.afterChild));
            }).ToList();

            var allTransitionChildren = new HashSet<Transform>(MorphTransitions
                .Concat(ScaleDownAndMoveTransitions)
                .SelectMany(i => new[] { i.beforeChild, i.afterChild }));

            foreach (Transform i in Before.transform)
                if (!allTransitionChildren.Contains(i))
                    _scaleDownTargets.Add(new Target(i));

            foreach (Transform i in After.transform)
                if (!allTransitionChildren.Contains(i))
                    _scaleUpTargets.Add(new Target(i));
        }

        /// <summary>Target Renderer with its original Transform values.</summary>
        private class Target
        {
            public readonly bool originalActive;
            public readonly Vector3 originalPosition;
            public readonly Vector3 originalScale;

            public readonly Transform transform;

            public Target(Transform transform)
            {
                this.transform = transform;
                originalScale = transform.localScale;
                originalPosition = transform.localPosition;
                originalActive = transform.gameObject.activeSelf;
            }

            public void ApplyOriginalValues()
            {
                transform.localPosition = originalPosition;
                transform.localScale = originalScale;
                transform.gameObject.SetActive(originalActive);
            }
        }

        private class MorphTarget : Target
        {
            public readonly Bounds originalSuperBounds;
            public readonly Vector3 originalWorldPosition;

            public MorphTarget(Transform transform) : base(transform)
            {
                originalSuperBounds = SuperBounds.GetSuperBounds(transform);
                originalWorldPosition = transform.position;
            }
        }
    }
}