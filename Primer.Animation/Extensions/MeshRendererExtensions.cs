using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class MeshRendererExtensions
    {
        private static readonly Dictionary<WeakReference<MeshRenderer>, Material> memory = new();

        public static Color GetColor(this MeshRenderer renderer)
        {
            return memory.TryGetValue(new WeakReference<MeshRenderer>(renderer), out var material)
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
            var weak = new WeakReference<MeshRenderer>(renderer);

            if (!memory.TryGetValue(weak, out var material)) {
                material = new Material(renderer.sharedMaterial);
                memory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }
    }
}
