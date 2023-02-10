using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class MeshRendererExtensions
    {
        private static readonly Dictionary<MeshRenderer, Material> stored = new();

        public static Color GetColor(this MeshRenderer renderer)
        {
            return stored.TryGetValue(renderer, out var material)
                ? material.color
                : renderer.sharedMaterial.color;
        }

        public static void SetColor(this MeshRenderer renderer, Color color)
        {
            Get(renderer).color = color;
        }

        public static async UniTask TweenColor(this MeshRenderer renderer, Color color, Tweener animation = null,
            CancellationToken ct = default)
        {
            var material = Get(renderer);

            if (material.color == color)
                return;

            await foreach (var lerpedColor in animation.Tween(material.color, color, ct)) {
                if (ct.IsCancellationRequested)
                    return;

                material.color = lerpedColor;
            }
        }

        private static Material Get(MeshRenderer renderer)
        {
            if (!stored.TryGetValue(renderer, out var material)) {
                material = new Material(renderer.sharedMaterial);
                stored.Add(renderer, material);
            }

            renderer.material = material;
            return material;
        }
    }
}
