using UnityEngine;

namespace Primer.Shapes
{
    public interface ILine
    {
        int numSegments { get; }
        Vector3[] points { get; }

        ILine ChangeResolution(int newResolution);
        ILine SmoothCut(float toResolution, bool fromOrigin);

        DiscreteLine ToDiscrete();

        public static ILine Lerp(ILine a, ILine b, float t) {
            var size = Mathf.Max(a.numSegments, b.numSegments);
            var length = size + 1;
            var finalPoints = new Vector3[length];

            if (a.numSegments != size) a = a.ChangeResolution(size);
            if (b.numSegments != size) b = b.ChangeResolution(size);

            for (var i = 0; i < length; i++) {
                finalPoints[i] = Vector3.Lerp(a.points[i], b.points[i], t);
            }

            return new DiscreteLine(finalPoints);
        }

        /// <summary>
        ///     Use this when resizing several grids at the same time
        ///     this ensures grids don't suffer more than one transformation
        /// </summary>
        public static (ILine, ILine) SameResolution(ILine a, ILine b) {
            var maxResolution = Mathf.Max(a.numSegments, b.numSegments);
            return (a.ChangeResolution(maxResolution), b.ChangeResolution(maxResolution));
        }
    }
}
