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
        IGrid Crop(float newSize, bool fromOrigin);


        public static Vector3[] Lerp(IGrid a, IGrid b, float t) {
            var maxSize = a.Size > b.Size ? a.Size : b.Size;
            var pointsPerSide = maxSize + 1;
            var length = pointsPerSide * pointsPerSide;
            var finalPoints = new Vector3[length];

            if (a.Size != maxSize) a = a.Resize(maxSize);
            if (b.Size != maxSize) b = b.Resize(maxSize);

            for (var i = 0; i < length; i++) {
                finalPoints[i] = Vector3.Lerp(a.Points[i], b.Points[i], t);
            }

            return finalPoints;
        }
    }
}
