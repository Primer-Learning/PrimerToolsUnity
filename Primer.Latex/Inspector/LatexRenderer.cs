using System;
using System.Collections.Generic;
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
    internal class LatexRenderer : IMeshController
    {
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

        [NonSerialized] internal Transform transform;
        [NonSerialized] internal LatexGroups latexGroups;

        private LatexExpression expression => latexGroups?.expression;

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

            if (isExpressionInvalid || transform == null || transform.gameObject.IsPreset())
                return;

            var zero = expression.center;
            var container = new Container(transform);

            foreach (var (start, end) in latexGroups.ranges) {
                var chunk = expression.Slice(start, end);
                var group = container.AddContainer($"Group (chars {start} to {end - 1})");
                var center = chunk.center;

                group.transform.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.transform.localScale = Vector3.one;
                group.transform.localRotation = Quaternion.identity;

                foreach (var character in chunk) {
                    var charTransform = group.Add($"LatexChar {character.position}");
                    charTransform.localScale = Vector3.one;
                    charTransform.localPosition = character.position - group.transform.localPosition;
                    charTransform.localRotation = Quaternion.identity;

                    var meshFilter = charTransform.GetOrAddComponent<MeshFilter>();
                    meshFilter.sharedMesh = character.mesh;

                    var meshRenderer = charTransform.GetOrAddComponent<MeshRenderer>();
                    meshRenderer.material = material;
                    meshRenderer.SetColor(color);
                }

                group.Purge();
            }

            container.Purge();
            
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

        MeshRenderer[] IMeshController.GetMeshRenderers()
        {
            return transform.GetComponentsInChildren<MeshRenderer>();
        }
    }
}
