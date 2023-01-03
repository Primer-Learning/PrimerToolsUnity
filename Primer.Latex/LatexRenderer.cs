using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();
        [HideInInspector] public List<int> groupIndexes = new();
        [SerializeField]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        internal List<string> headers = LatexInput.GetDefaultHeaders();
        [SerializeField] [TextArea]
        internal string latex = "";
        public Material material;
        public UnityEvent<LatexChar[]> onChange = new();

        public LatexInput config => new(latex, headers);


        public async Task Process(LatexInput input)
        {
            var prevCharacters = characters;
            characters = await processor.Process(input);

            if (!prevCharacters.SequenceEqual(characters))
                onChange.Invoke(characters);
        }


#if UNITY_EDITOR

        // This needs to be private (or internal) because SpriteDirectRenderer is internal
        [Tooltip("Which mesh features to visualize. Gizmos are only ever visible in the Unity editor.")]
        [SerializeField]
        internal LatexGizmoMode gizmos = LatexGizmoMode.Nothing;

        internal List<(int, int)> ranges => characters.GetRanges(groupIndexes);

        private void OnDrawGizmos()
        {
            for (var i = 0; i < characters.Length; i++) {
                characters[i].DrawWireGizmos(transform, gizmos);
            }
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
            UpdateChildren();
        }

        public void UpdateChildren()
        {
            var ranges = characters.GetRanges(groupIndexes);

            if (characters.Length == 0 || IsEmptyRange(ranges))
                return;

            var zero = characters.GetCenter();
            var groups = new ChildrenModifier(transform);

            foreach (var (start, end) in ranges) {
                var group = groups.NextMustBeCalled($"Group (chars {start} to {end - 1})");
                var children = new ChildrenModifier(group);
                var chars = characters.Skip(start).Take(end - start).ToArray();
                var center = chars.GetCenter();

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));

                foreach (var character in chars) {
                    var charTransform = children.NextMustBeCalled($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - group.localPosition; //- center + zero;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.symbol.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                children.Apply();
            }

            groups.Apply();
        }

        private static bool IsEmptyRange(List<(int start, int end)> ranges)
            => (ranges.Count == 1) && (ranges[0] == (0, 0));
#endif
    }
}
