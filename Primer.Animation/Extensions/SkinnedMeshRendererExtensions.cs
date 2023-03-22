using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public static class SkinnedMeshRendererExtensions
    {
        private static readonly Dictionary<WeakReference<SkinnedMeshRenderer>, Material> memory = new();

        public static Color GetColor(this SkinnedMeshRenderer renderer)
        {
            return memory.TryGetValue(new WeakReference<SkinnedMeshRenderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial.color;
        }

        public static void SetColor(this SkinnedMeshRenderer renderer, Color color)
        {
            if (renderer.sharedMaterial == null)
                return;

            Get(renderer).color = color;
        }

        public static Tween TweenColor(this SkinnedMeshRenderer renderer, Color color)
        {
            var material = Get(renderer);

            return material.color == color
                ? Tween.noop
                : new Tween(t => material.color = Color.Lerp(material.color, color, t));
        }

        private static Material Get(SkinnedMeshRenderer renderer)
        {
            if (renderer.sharedMaterial == null)
                throw new InvalidOperationException("SkinnedMeshRenderer doesn't have a material");

            var weak = new WeakReference<SkinnedMeshRenderer>(renderer);

            if (!memory.TryGetValue(weak, out var material)) {
                material = new Material(renderer.sharedMaterial);
                memory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }
    }
}
