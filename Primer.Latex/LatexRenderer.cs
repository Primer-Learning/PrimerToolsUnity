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
    [AddComponentMenu("Primer/Latex Renderer")]
    public class LatexRenderer : MonoBehaviour
    {
        [SerializeField] [TextArea]
        internal string latex = "";

        [SerializeField]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        internal List<string> headers = LatexInput.GetDefaultHeaders();
        public Material material;
        [HideInInspector] public List<int> groupIndexes = new();
        public UnityEvent<LatexChar[]> onChange = new();

        public LatexInput config => new(latex, headers);


        public async Task Process(LatexInput input)
        {
            var prevCharacters = characters;
            characters = await processor.Process(input);

            if (!prevCharacters.SequenceEqual(characters))
                onChange.Invoke(characters);
        }

        private void Render()
        {
            for (var i = 0; i < characters.Length; i++) {
                characters[i].Draw(transform, material);
            }
        }

        #region Internal fields
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();
        [NotNull] internal LatexChar[] characters = Array.Empty<LatexChar>();

        internal bool isValid => (characters.Length > 0) && characters.All(x => x.isSpriteValid);
        internal bool hasContent => !config.IsEmpty || characters.Length > 0;
        #endregion


        #region Unity events
        private async void OnEnable()
        {
            if (!hasContent || isValid)
                return;

            await Process(config);

            // the component is destroyed after calling OnEnabled() when starting play mode
            if (this != null)
                LateUpdate();
        }

        // We mess with the update loop when rendering the sprites
        // so LateUpdate is required here
        private void LateUpdate()
        {
            if (hasContent && isValid && (transform.childCount == 0))
                Render();
        }
        #endregion


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

        public void UpdateChildren()
        {
            var ranges = characters.GetRanges(groupIndexes);
            var zero = characters.GetCenter();
            var groups = new ChildrenModifier(transform);

            foreach (var (start, end) in ranges) {
                var group = groups.NextMustBeCalled($"Group (chars {start} to {end - 1})");
                var children = new ChildrenModifier(group);
                var chars = characters.Skip(start).Take(end - start).ToArray();
                var center = chars.GetCenter();

                group.localPosition = center - zero;

                foreach (var character in chars) {
                    var charTransform = children.NextMustBeCalled($"LatexChar {character.position}");
                    charTransform.localPosition = character.position - center + zero;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.symbol.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                }

                children.Apply();
            }

            groups.Apply();
        }
#endif
    }
}
