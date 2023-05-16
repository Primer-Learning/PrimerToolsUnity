using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Primer.Latex
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Rendering")]
    internal class LatexRenderer : IMeshRendererController
    {
        [SerializeField]
        [HideInInspector]
        private Color _color = Color.white;

        [ShowInInspector]
        public Color color {
            get => _color;
            set {
                _color = value;
                this.SetColor(value);
            }
        }

        [SerializeField]
        private Material _material;

        public Material material {
            get => _material ??= IMeshRendererController.defaultMaterial;
            set {
                _material = value;
                this.SetMaterial(value);
            }
        }


        [NonSerialized] internal Transform transform;
        [NonSerialized] internal LatexGroups latexGroups;

        private LatexExpression expression => latexGroups?.expression;

        MeshRenderer[] IMeshRendererController.meshRenderers => transform.GetComponentsInChildren<MeshRenderer>();


        public void SetColorInGroups(Color groupColor, params int[] groupIndexes)
        {
            var children = groupIndexes
                .Select(index => transform.GetChild(index))
                .SelectMany(group => group.GetComponentsInChildren<MeshRenderer>());

            foreach (var child in children)
                child.SetColor(groupColor);
        }

        public Tween TweenColorInGroups(Color groupColor, IEnumerable<int> groupIndexes)
        {
            var children = groupIndexes
                .Select(index => transform.GetChild(index))
                .SelectMany(group => group.GetComponentsInChildren<MeshRenderer>())
                .ToArray();

            if (children.Length == 0)
                return Tween.noop;

            var initial = children[0].GetColor();

            if (initial == groupColor)
                return Tween.noop;

            return new Tween(t => {
                var lerpedColor = Color.Lerp(initial, groupColor, t);

                foreach (var child in children)
                    child.SetColor(lerpedColor);
            });
        }

        public Tween TweenColorByCharacterRange(Color targetColor, int startIndex, int endIndex)
        {
            var targetChildren = transform
                .GetChildren()
                .SelectMany(x => x.GetChildren()).Take(endIndex + 1).Skip(startIndex)
                .Select(x => x.GetComponent<MeshRenderer>())
                .ToArray();

            var initial = targetChildren[0].GetColor();

            if (initial == targetColor)
                return Tween.noop;

            // This is the memory-leak-producing way to tween a color, but I wanted to use Tween.Value so 
            // the initial values are correct even when in the same clip as another tween that affects the same value.
            return Tween.Parallel(
                targetChildren.Select(r => Tween.Value(() => r.material.color, targetColor))
            );
        }
        
        public void SetCastShadows(bool castShadows)
        {
            var targetChildren = transform
                .GetChildren()
                .SelectMany(x => x.GetChildren())
                .Select(x => x.GetComponent<MeshRenderer>())
                .ToArray();

            foreach (var child in targetChildren)
            {
                child.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }

        public void UpdateChildren()
        {
            var isExpressionInvalid = expression is null || expression.Any(x => x.mesh is null);

            if (isExpressionInvalid || transform.gameObject.IsPreset())
                return;

            var zero = expression.center;
            var children = new ChildrenDeclaration(
                transform,
                onRemove: x => x.Dispose(urgent: true)
            );

            foreach (var (start, end) in latexGroups.ranges) {
                var chunk = expression.Slice(start, end);
                var group = children.Next($"Group (chars {start} to {end - 1})");
                var center = chunk.center;
                var grandChildren = new ChildrenDeclaration(group);

                group.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.localScale = Vector3.one;
                group.localRotation = Quaternion.identity;

                foreach (var character in chunk) {
                    var charTransform = grandChildren.Next($"LatexChar {character.position}");
                    charTransform.localScale = Vector3.one;
                    charTransform.localPosition = character.position - group.localPosition;
                    charTransform.localRotation = Quaternion.identity;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                    meshRenderer.SetColor(color);
                }

                grandChildren.Apply();
            }

            children.Apply();
            
            // Just make sure all shadows are off for now. Could make this an option in the future if needed.
            SetCastShadows(false);
        }

        public void DrawGizmos()
        {
            if (expression is null)
                return;

            var rootPosition = transform.position;
            var zero = expression.center;

            foreach (var (start, end) in latexGroups.ranges) {
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
    }
}
