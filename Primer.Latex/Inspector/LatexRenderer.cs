using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Primer.Latex
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Rendering")]
    internal class LatexRenderer
    {
        [SerializeField]
        [HideInInspector]
        private Color _color = Color.white;

        [ShowInInspector]
        public Color color {
            get => _color;
            set {
                _color = value;

                foreach (var mesh in meshRenderers)
                    mesh.SetColor(value);
            }
        }

        [SerializeField]
        private Material _material;

        public Material material {
            get => _material ??= LatexComponent.defaultMaterial;
            set {
                _material = value;

                foreach (var mesh in meshRenderers)
                    mesh.material = value;
            }
        }


        [NonSerialized] internal Transform transform;
        [NonSerialized] internal LatexGroups latexGroups;

        private LatexExpression expression => latexGroups?.expression;
        private MeshRenderer[] meshRenderers => transform.GetComponentsInChildren<MeshRenderer>();


        public async UniTask TweenColor(Color newColor, Tweener animation = null, CancellationToken ct = default)
        {
            if (color == newColor)
                return;

            var children = meshRenderers;

            await foreach (var lerpedColor in animation.Tween(color, newColor, ct)) {
                if (ct.IsCancellationRequested)
                    return;

                color = lerpedColor;

                foreach(var child in children)
                    child.SetColor(lerpedColor);
            }
        }

        public void UpdateChildren()
        {
            if (expression is null || expression.Any(x => x.mesh is null))
                return;

            var children = new ChildrenDeclaration(transform);
            var zero = expression.center;

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
