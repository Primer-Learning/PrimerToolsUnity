using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;

namespace Primer.Latex
{
    [ExecuteAlways]
    [HideMonoScript]
    [AddComponentMenu("Primer / Latex Renderer")]
    public class LatexRenderer : GeneratorBehaviour
    {
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();

        [NonSerialized]
        internal LatexExpression expression;

        [SerializeField]
        [HideInInspector]
        internal List<int> groupIndexes = new();

        [SerializeField]
        [PropertyOrder(2)]
        [HideLabel, Title("LaTeX")]
        [PropertySpace(SpaceBefore = 32, SpaceAfter = 32)]
        [Multiline(10)]
        internal string latex = "";

        [FoldoutGroup("Details", order: 10)]
        public Material material;

        [SerializeField]
        [FoldoutGroup("Details")]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        internal List<string> headers = LatexInput.GetDefaultHeaders();

        [FoldoutGroup("Details")]
        public UnityEvent<LatexExpression> onChange = new();


        public LatexInput config => new(latex, headers);

        internal bool isRunning => processor.state == LatexProcessingState.Processing;

        [NonSerialized]
        private LatexTransitionState stateCache;
        internal LatexTransitionState state => stateCache ??= new LatexTransitionState(
            this,
            expression.Split(groupIndexes)
        );


        private new async void OnValidate()
        {
            await Process(config);

            // Comment this to disable automatic children update
            if (this)
                base.OnValidate();
        }


        #region Latex processing
        public UniTask Process(int input) => Process($"${input}$");
        public UniTask Process(float input) => Process($"${input}$");
        public UniTask Process(string input) => Process(new LatexInput(input, headers));
        public async UniTask Process(LatexInput input)
        {
            latex = input.code;
            headers = input.headers;
            var newExpression = await Evaluate(input);
            ApplyExpression(newExpression);
        }

        public async UniTask Replace(string input)
        {
            var primer = this.GetPrimer();
            var processing = Evaluate(new LatexInput(input, headers));
            await primer.ScaleDownToZero();
            var newExpression = await processing;
            ApplyExpression(newExpression);
            await primer.ScaleUpFromZero();
        }


        private async UniTask<LatexExpression> Evaluate(LatexInput input)
        {
            try {
                return await processor.Process(input);
            }
            catch (OperationCanceledException) {
                Debug.LogWarning($"Removing queued LaTeX execution: {input.code}");
                return null;
            }
        }

        private bool ApplyExpression(LatexExpression newExpression)
        {
            if (newExpression is null || expression is not null && expression.IsSame(newExpression))
                return false;

            expression = newExpression;
            UpdateChildren();
            onChange.Invoke(expression);
            return true;
        }
        #endregion

        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration children)
        {
            if (expression is null || expression.Any(x => x.mesh is null)) {
                CancelCurrentUpdate();
                return;
            }

            var zero = expression.center;

            foreach (var (start, end) in expression.CalculateRanges(groupIndexes)) {
                var chunk = expression.Slice(start, end);
                var group = children.Next($"Group (chars {start} to {end - 1})");
                var center = chunk.center;
                var grandChildren = new ChildrenDeclaration(group);

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.localScale = Vector3.one;
                group.localRotation = Quaternion.identity;

                foreach (var character in chunk) {
                    var charTransform = grandChildren.Next($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - group.localPosition;
                    charTransform.localRotation = Quaternion.identity;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                grandChildren.Apply();
            }
        }


#if UNITY_EDITOR
        public List<(int start, int end)> ranges => expression?.CalculateRanges(groupIndexes);

        private void Reset()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }

        internal void InvalidateCache()
        {
            stateCache = null;
        }

        private void Update()
        {
            if (transform.hasChanged) {
                InvalidateCache();
                transform.hasChanged = false;
            }
        }

        [ResponsiveButtonGroup(Order = 1)]
        [Button("Open build dir")]
        private void OpenBuildDir() => processor.OpenBuildDir();

        [ResponsiveButtonGroup]
        [Button("Open cache dir")]
        private void OpenCacheDir() => LatexProcessingCache.OpenCacheDir();

        [EnableIf(nameof(isRunning))]
        [ResponsiveButtonGroup]
        [Button("Cancel Rendering")]
        private void Cancel() => processor.Cancel();


        private void OnDrawGizmos()
        {
            if (expression is null)
                return;

            var rootPosition = transform.position;
            var zero = expression.center;

            foreach (var (start, end) in ranges) {
                var chunk = expression.Slice(start, end);
                var bounds = chunk.GetBounds();
                var center = chunk.center;
                var position = Vector3.Scale(center - zero, new Vector3(1, -1, 1));

                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(rootPosition + position, bounds.size);
                Handles.Label(rootPosition + position, $"Group {start} to {end - 1}");

                foreach (var character in chunk) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(character.position + rootPosition, character.bounds.size);
                }
            }
        }

#endif
    }
}
