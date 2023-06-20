using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Rendering;

namespace Primer.Latex
{
    public partial class LatexComponent : IMeshController
    {
        private const string CHARACTERS_CONTAINER_NAME = "Characters";

        private Transform charactersContainer => transform.FindOrCreate(CHARACTERS_CONTAINER_NAME);

        [Title("Rendering")]
        [ShowInInspector]
        public Color color {
            get => this.GetColor();
            set => this.SetColor(value);
        }

        [ShowInInspector]
        public Material material {
            get => this.GetMaterial();
            set => this.SetMaterial(value);
        }


        private void Reset() => PatchMaterial();

        private void PatchMaterial()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (material is null || presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = MeshRendererExtensions.defaultMaterial;
            }
        }


        public IEnumerable<MeshRenderer> GetCharacters(int? startIndex = null, int? endIndex = null)
        {
            var chars = charactersContainer.GetChildren();
            var cropped = endIndex.HasValue ? chars.Take(endIndex.Value + 1) : chars;
            return cropped.Skip(startIndex ?? 0).Select(x => x.GetComponent<MeshRenderer>());
        }

        public void SetColors(Color newColor, int? startIndex = null, int? endIndex = null)
        {
            GetCharacters(startIndex, endIndex).SetColor(newColor);
        }

        public Tween TweenColors(Color newColor, int? startIndex = null, int? endIndex = null)
        {
            return GetCharacters(startIndex, endIndex).TweenColor(newColor);
        }

        public void SetCastShadows(bool castShadows)
        {
            var mode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            foreach (var child in GetCharacters()) {
                child.shadowCastingMode = mode;
            }
        }


        [PropertySpace]
        [ButtonGroup("Characters render")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.ArrowRepeat)]
        public void UpdateCharacters()
        {
            if (activeDisplay is null)
                SetActiveDisplay(charactersContainer);

            var isExpressionInvalid = expression is null || expression.Any(x => x.mesh is null);

            if (isExpressionInvalid || transform == null || transform.gameObject.IsPreset())
                return;

            var container = new Container(charactersContainer);
            var currentMaterial = material;
            var currentColor = color;

            foreach (var (index, character) in expression.WithIndex()) {
                var charTransform = container.Add($"LatexChar {index}").SetDefaults();
                character.RenderTo(charTransform, currentMaterial, currentColor);
            }

            container.Purge();

            // Just make sure all shadows are off for now. Could make this an option in the future if needed.
            SetCastShadows(false);
        }

        [ButtonGroup("Characters render")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Trash)]
        protected virtual void RegenerateCharacters()
        {
            if (gameObject.IsPreset())
                return;

            transform.RemoveAllChildren();
            UpdateCharacters();
        }

        MeshRenderer[] IMeshController.GetMeshRenderers()
        {
            return charactersContainer.GetComponentsInChildren<MeshRenderer>();
        }
    }
}
