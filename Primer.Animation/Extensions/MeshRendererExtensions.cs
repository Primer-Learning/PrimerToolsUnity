using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public static class MeshRendererExtensions
    {
        private static readonly Dictionary<WeakReference<MeshRenderer>, Material> memory = new();
        private static readonly Dictionary<WeakReference<SkinnedMeshRenderer>, Material> skinnedMemory = new();

        public static Color GetColor(this MeshRenderer renderer)
        {
            return memory.TryGetValue(new WeakReference<MeshRenderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial.color;
        }

        public static Color GetColor(this SkinnedMeshRenderer renderer)
        {
            return skinnedMemory.TryGetValue(new WeakReference<SkinnedMeshRenderer>(renderer), out var material)
                ? material.color
                : renderer.sharedMaterial.color;
        }

        public static void SetColor(this MeshRenderer renderer, Color color)
        {
            if (renderer.sharedMaterial is null)
                return;

            Get(renderer).color = color;
        }

        public static void SetColor(this SkinnedMeshRenderer renderer, Color color)
        {
            if (renderer.sharedMaterial == null)
                return;

            GetSkinned(renderer).color = color;
        }

        public static Tween TweenColor(this MeshRenderer renderer, Color color)
        {
            var material = Get(renderer);

            return material.color == color
                ? Tween.noop
                : new Tween(t => material.color = Color.Lerp(material.color, color, t));
        }

        public static Tween TweenColor(this SkinnedMeshRenderer renderer, Color color)
        {
            var material = GetSkinned(renderer);

            return material.color == color
                ? Tween.noop
                : new Tween(t => material.color = Color.Lerp(material.color, color, t));
        }

        private static Material Get(MeshRenderer renderer)
        {
            if (renderer.sharedMaterial == null)
                throw new InvalidOperationException("MeshRenderer doesn't have a material");

            var weak = new WeakReference<MeshRenderer>(renderer);

            if (!memory.TryGetValue(weak, out var material)) {
                material = new Material(renderer.sharedMaterial);
                memory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }

        private static Material GetSkinned(SkinnedMeshRenderer renderer)
        {
            if (renderer.sharedMaterial == null)
                throw new InvalidOperationException("SkinnedMeshRenderer doesn't have a material");

            var weak = new WeakReference<SkinnedMeshRenderer>(renderer);

            if (!skinnedMemory.TryGetValue(weak, out var material)) {
                material = new Material(renderer.sharedMaterial);
                skinnedMemory.Add(weak, material);
            }

            renderer.material = material;
            return material;
        }
    }
}
