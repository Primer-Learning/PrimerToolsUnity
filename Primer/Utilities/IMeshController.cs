using UnityEngine;

namespace Primer
{
    public interface IMeshController
    {
        public MeshRenderer[] GetMeshRenderers();
    }


    public static class IMeshControllerExtensions
    {
        public static Color GetColor(this IMeshController self)
        {
            return self.GetMaterial().color;
        }

        public static void SetColor(this IMeshController self, Color color)
        {
            foreach (var mesh in self.GetMeshRenderers()) {
                mesh.SetColor(color);
            }
        }

        public static Material GetMaterial(this IMeshController self)
        {
            var renderers = self.GetMeshRenderers();

            return renderers.Length is 0
                ? MeshRendererExtensions.defaultMaterial
                : renderers[0].material;
        }

        public static void SetMaterial(this IMeshController self, Material material)
        {
            foreach (var mesh in self.GetMeshRenderers()) {
                mesh.material = material;
            }
        }
    }
}
