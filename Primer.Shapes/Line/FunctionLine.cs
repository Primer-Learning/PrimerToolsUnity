using System;
using UnityEngine;

namespace Primer.Shapes
{
    public readonly struct FunctionLine : ILine
    {
        private const int DEFAULT_RESOLUTION = 10;
        private const float DEFAULT_START = 0;
        private const float DEFAULT_END = 1;
        private readonly Func<float, Vector3> function;

        public static FunctionLine Default()
        {
            return new FunctionLine(x => Vector3.zero) {
                resolution = DEFAULT_RESOLUTION,
                xStart = DEFAULT_START,
                xEnd = DEFAULT_END,
            };
        }

        public int resolution { get; init; }
        public float xStart { get; init; }
        public float xEnd { get; init; }
        public Vector3[] points => CalculatePoints();

        public FunctionLine(Func<float, Vector3> function)
        {
            resolution = DEFAULT_RESOLUTION;
            xStart = DEFAULT_START;
            xEnd = DEFAULT_END;
            this.function = function;
        }

        public FunctionLine(Func<float, Vector2> function)
        {
            resolution = DEFAULT_RESOLUTION;
            xStart = DEFAULT_START;
            xEnd = DEFAULT_END;
            this.function = x => function(x);
        }

        public FunctionLine(Func<float, float> function)
        {
            resolution = DEFAULT_RESOLUTION;
            xStart = DEFAULT_START;
            xEnd = DEFAULT_END;
            this.function = x => new Vector3(x, function(x), 0);
        }

        public ILine ChangeResolution(int newResolution)
        {
            if (newResolution == resolution)
                return this;

            return new FunctionLine(function) {
                resolution = newResolution,
                xStart = xStart,
                xEnd = xEnd,
            };
        }

        // public ILine Crop(int maxResolution, bool fromOrigin)
        // {
        //     var xDelta = PrimerMath.Remap(0, resolution, xEnd, xStart, maxResolution);
        //
        //     return new FunctionLine(function) {
        //         resolution = maxResolution,
        //         xStart = fromOrigin ? xStart + xDelta : xStart,
        //         xEnd = fromOrigin ? xEnd : xEnd - xDelta,
        //     };
        // }

        public ILine SmoothCut(float toResolution, bool fromOrigin)
        {
            return ToDiscrete().SmoothCut(toResolution, fromOrigin);
        }

        public DiscreteLine ToDiscrete()
        {
            return new DiscreteLine(points);
        }

        private Vector3[] CalculatePoints()
        {
            var result = new Vector3[resolution + 1];
            var step = (xEnd - xStart) / resolution;
            var x = xStart;

            for (var i = 0; i <= resolution; i++) {
                result[i] = function(x);
                x += step;
            }

            return result;
        }
    }
}
