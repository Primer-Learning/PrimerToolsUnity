using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Primer.Animation
{
    public interface IMeshRendererController
    {
        private static Material defaultMaterialCache;
        public static Material defaultMaterial =>
            defaultMaterialCache ??= AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

        protected internal MeshRenderer[] meshRenderers { get; }
        public Color color { get; set; }
        public Material material { get; set; }
    }

    public static class ColorControlExtensions
    {
        public static void SetMaterial(this IMeshRendererController control, Material material)
        {
            foreach (var mesh in control.meshRenderers) {
                mesh.material = material;
            }
        }

        public static void SetColor(this IMeshRendererController control, Color color)
        {
            foreach (var mesh in control.meshRenderers) {
                mesh.SetColor(color);
            }
        }

        public static async UniTask TweenColor(this IMeshRendererController control, Color newColor,
            Tweener animation = null, CancellationToken ct = default)
        {
            if (control.color == newColor)
                return;

            var children = control.meshRenderers;

            await foreach (var lerpedColor in animation.Tween(control.color, newColor, ct)) {
                if (ct.IsCancellationRequested)
                    return;

                foreach (var child in children)
                    child.SetColor(lerpedColor);
            }

            if (ct.IsCancellationRequested)
                control.color = newColor;
        }
    }
}
