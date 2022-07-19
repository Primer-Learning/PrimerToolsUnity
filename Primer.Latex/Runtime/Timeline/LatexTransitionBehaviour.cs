using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace LatexRenderer.Timeline
{
    public class LatexTransitionBehaviour : PlayableBehaviour
    {
        private List<(Target beforeChild, Target afterChild)> _morphTargets = new();
        private float _scaleDownDuration = 0.1f;
        private List<Target> _scaleDownTargets = new();
        private float _scaleUpDuration = 0.1f;
        private List<Target> _scaleUpTargets = new();

        public Transform After;
        public Transform AfterAnchor;
        public Transform Before;
        public Transform BeforeAnchor;
        public List<(Transform beforeChild, Transform afterChild)> MorphTransitions;

        public float ScaleDownDuration
        {
            get => _scaleDownDuration;
            set
            {
                if (value is < 0f or > 1f)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Must be time ratio between 0 and 1 inclusive.");

                _scaleDownDuration = value;
            }
        }

        public float ScaleUpDuration
        {
            get => _scaleUpDuration;
            set
            {
                if (value is < 0f or > 1f)
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Must be time ratio between 0 and 1 inclusive.");

                _scaleUpDuration = value;
            }
        }

        private void UpdateScaleDowns(float timeRatio)
        {
            var scaleDownTimeRatio = timeRatio / ScaleDownDuration;
            foreach (var i in _scaleDownTargets)
                i.transform.localScale = Vector3.Lerp(i.originalScale, Vector3.zero,
                    scaleDownTimeRatio);
        }

        private void UpdateScaleUps(float timeRatio)
        {
            var scaleUpTimeRatio = (timeRatio - (1 - ScaleUpDuration)) / ScaleUpDuration;
            foreach (var i in _scaleUpTargets)
                i.transform.localScale = Vector3.Lerp(Vector3.zero, i.originalScale,
                    scaleUpTimeRatio);
        }

        private void UpdateMorphs(float timeRatio)
        {
            // var morphTimeRatio =
            // foreach (var (beforeChild, afterChild) in _morphTargets)
            // {
            //     beforeChild.transform.localPosition = Vector3.Lerp()
            // }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var timeRatio = (float)(playable.GetTime() / playable.GetDuration());
            UpdateScaleDowns(timeRatio);
            UpdateScaleUps(timeRatio);
            UpdateMorphs(timeRatio);
        }

        private static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }

        private void ResetTargets()
        {
            _morphTargets = new List<(Target beforeChild, Target afterChild)>();
            _scaleDownTargets = new List<Target>();
            _scaleUpTargets = new List<Target>();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            var allTargets =
                Concatenate(_morphTargets.SelectMany(i => new[] { i.beforeChild, i.afterChild }),
                    _scaleDownTargets, _scaleUpTargets);
            foreach (var target in allTargets) target.ApplyOriginalTransform();

            ResetTargets();
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            ResetTargets();

            _morphTargets = MorphTransitions.Select(i =>
            {
                if (i.beforeChild.parent != Before)
                    throw new Exception("BeforeChild of morph transition is not child of Before.");

                if (i.afterChild.parent != After)
                    throw new Exception("AfterChild of morph transition is not child of After.");

                return (Target.FromTransform(i.beforeChild), Target.FromTransform(i.afterChild));
            }).ToList();

            var allMorphChildren =
                new HashSet<Transform>(
                    MorphTransitions.SelectMany(i => new[] { i.beforeChild, i.afterChild }));

            foreach (Transform i in Before.transform)
                if (!allMorphChildren.Contains(i))
                    _scaleDownTargets.Add(Target.FromTransform(i));

            foreach (Transform i in After.transform)
                if (!allMorphChildren.Contains(i))
                    _scaleUpTargets.Add(Target.FromTransform(i));
        }

        /// <summary>Target Renderer with its original Transform values.</summary>
        private class Target
        {
            public Vector3 originalPosition;
            public Vector3 originalScale;

            public Transform transform;

            public void ApplyOriginalTransform()
            {
                transform.localPosition = originalPosition;
                transform.localScale = originalScale;
            }

            public static Target FromTransform(Transform transform)
            {
                return new Target
                {
                    transform = transform,
                    originalScale = transform.localScale,
                    originalPosition = transform.localPosition
                };
            }
        }
    }
}