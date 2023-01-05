using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer / Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();
        internal LatexExpression expression = new();


        [SerializeField]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        internal List<string> headers = LatexInput.GetDefaultHeaders();
        [SerializeField] [TextArea]
        internal string latex = "";
        public Material material;

        public UnityEvent<LatexExpression> onChange = new();


        public async Task Process(LatexInput input)
        {
            var prevExpression = expression;
            expression = await processor.Process(input);

            if (!prevExpression.IsSame(expression))
                onChange.Invoke(expression);
        }


        #region Group management
        public LatexInput config => new(latex, headers);

        private LatexTransitionState stateCache;

        internal LatexTransitionState state => stateCache ??= new LatexTransitionState(
            transform,
            expression.Split(groupIndexes)
        );

        [SerializeField] [HideInInspector]
        internal List<int> groupIndexesInternal = new();
        public List<int> groupIndexes {
            get => groupIndexesInternal;
            set {
                if (value is null || groupIndexesInternal.SequenceEqual(value))
                    return;

                groupIndexesInternal = value;

                stateCache = new LatexTransitionState(
                    transform,
                    expression.Split(groupIndexesInternal)
                );
            }
        }
        #endregion


#if UNITY_EDITOR

        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        internal LatexGizmoMode gizmos = LatexGizmoMode.Nothing;

        public List<(int start, int end)> ranges => expression.CalculateRanges(groupIndexes);

        private void OnDrawGizmos()
        {
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

        private async void OnValidate()
        {
            await Process(config);

            if (this)
                UpdateChildren();
        }

        public void UpdateChildren()
        {
            var zero = expression.GetCenter();
            var groupGameObjects = new ChildrenModifier(transform);

            foreach (var (start, end) in expression.CalculateRanges(groupIndexes)) {
                var chunk = expression.Slice(start, end);
                var group = groupGameObjects.NextMustBeCalled($"Group (chars {start} to {end - 1})");
                var children = new ChildrenModifier(group);
                var center = chunk.GetCenter();

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));

                foreach (var character in chunk) {
                    var charTransform = children.NextMustBeCalled($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - group.localPosition; //- center + zero;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.symbol.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                children.Apply();
            }

            groupGameObjects.Apply();
        }
#endif
    }
}
