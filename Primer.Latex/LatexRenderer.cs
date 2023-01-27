using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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


        public Task Process(int input) => Process($"${input}$");
        public Task Process(float input) => Process($"${input}$");
        public Task Process(string input)
        {
            latex = input;
            return Process(config);
        }

        public async Task Process(LatexInput input)
        {
            var prevExpression = expression;

            try {
                expression = await processor.Process(input);

                if (prevExpression is null || !prevExpression.IsSame(expression)) {
                    UpdateChildren();
                    onChange.Invoke(expression);
                }
            }
            catch (OperationCanceledException) {
                Debug.LogWarning($"Removing queued LaTeX execution: {input.code}");
            }
        }

        protected override void UpdateChildren(bool isEnabled, ChildrenDeclaration children)
        {
            if (expression is null || expression.Any(x => x.symbol.mesh is null)) {
                CancelCurrentUpdate();
                return;
            }

            var zero = expression.GetCenter();

            foreach (var (start, end) in expression.CalculateRanges(groupIndexes)) {
                var chunk = expression.Slice(start, end);
                var group = children.Next($"Group (chars {start} to {end - 1})");
                var center = chunk.GetCenter();
                var grandChildren = new ChildrenDeclaration(group);

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.localScale = Vector3.one;
                group.localRotation = Quaternion.identity;

                foreach (var character in chunk) {
                    var charTransform = grandChildren.Next($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - group.localPosition;
                    charTransform.localRotation = Quaternion.identity;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.symbol.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                grandChildren.Apply();
            }
        }


#if UNITY_EDITOR
        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [SerializeField]
        [FoldoutGroup("Details")]
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        internal LatexGizmoMode gizmos = LatexGizmoMode.Nothing;

        public List<(int start, int end)> ranges => expression?.CalculateRanges(groupIndexes);

        private void OnDrawGizmos()
        {
            if (expression is null)
                return;

            foreach (var character in expression)
                character.DrawWireGizmos(transform, gizmos);
        }

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
        [Button("Open Build Directory")]
        private void OpenBuildDir() => processor.OpenBuildDir();

        [DisableIf(nameof(isRunning))]
        [ResponsiveButtonGroup]
        [Button("Cancel Rendering Task")]
        private void Cancel() => processor.Cancel();

        [ResponsiveButtonGroup]
        [Button("Update children")]
        private void RunUpdateChildren() => UpdateChildren();
#endif
    }
}
