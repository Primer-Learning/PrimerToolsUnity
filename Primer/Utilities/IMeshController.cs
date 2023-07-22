using UnityEngine;

namespace Primer
{
    /// <summary>
    ///     Implement this interface to get methods like `SetColor()` and `SetMaterial()` in your class
    /// </summary>
    public interface IMeshController
    {
        public MeshRenderer[] GetMeshRenderers();
    }


    public static class IMeshControllerExtensions
    {
        public static Color GetColor(this IMeshController self)
        {
            return self.GetMeshRenderers().GetColor();
        }

        public static void SetColor(this IMeshController self, Color color)
        {
            self.GetMeshRenderers().SetColor(color);
        }

        public static Material GetMaterial(this IMeshController self)
        {
            return self.GetMeshRenderers().GetMaterial();
        }

        public static void SetMaterial(this IMeshController self, Material material)
        {
            self.GetMeshRenderers().SetMaterial(material);
        }
    }
}
