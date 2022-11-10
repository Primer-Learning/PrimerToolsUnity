using UnityEngine;

namespace Primer.Graph
{
    public interface IGrid
    {
        int Size { get; }
        Vector3[] Points { get; }

        void RenderTo(Mesh mesh, bool bothSides);

        IGrid Resize(int newSize);
        IGrid Crop(float newSize);
    }
}
