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
            return self.GetMeshRenderers()[0].GetColor();
        }

        public static void SetColor(this IMeshController self, Color color)
        {
            foreach (var mesh in self.GetMeshRenderers()) {
                mesh.SetColor(color);
            }
        }

        public static Material GetMaterial(this IMeshController self)
        {
            return self.GetMeshRenderers()[0].sharedMaterial;
        }

        public static void SetMaterial(this IMeshController self, Material material)
        {
            foreach (var mesh in self.GetMeshRenderers()) {
                mesh.material = material;
            }
        }
    }
}
