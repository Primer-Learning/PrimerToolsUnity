using UnityEngine;

namespace Primer.Graph
{
    public interface ILine
    {
        int Length { get; }
        Vector3[] Points { get; }

        ILine Resize(int newLength);
        ILine Crop(float newLength);
        ILine Crop(float newLength, bool fromOrigin);


        public static Vector3[] Lerp(ILine a, ILine b, float t) {
            var length = a.Length > b.Length ? a.Length : b.Length;
            var result = new Vector3[length];

            if (a.Length != length) a = a.Resize(length);
            if (b.Length != length) b = b.Resize(length);

            for (var i = 0; i < length; i++) {
                result[i] = Vector3.Lerp(a.Points[i], b.Points[i], t);
            }

            return result;
        }
    }

}
