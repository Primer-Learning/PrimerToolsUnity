using System;
using UnityEngine;

namespace Primer.Shapes
{
    public readonly struct FunctionGrid : IGrid
    {
        public static readonly Vector2Int defaultResolution = Vector2Int.one * 10;
        public static readonly Vector2 defaultStart = Vector2.zero;
        public static readonly Vector2 defaultEnd = Vector2.one;

        public static FunctionGrid Default()
        {
            return new FunctionGrid((x, z) => Vector3.zero) {
                resolution = defaultResolution,
                start = defaultStart,
                end = defaultEnd,
            };
        }

        private readonly Func<float, float, Vector3> function;

        public Vector2Int resolution { get; init; }
        public Vector2 start { get; init; }
        public Vector2 end { get; init; }
        public Vector3[,] points => CalculatePoints();

        public FunctionGrid(Func<float, float, Vector3> function)
        {
            resolution = defaultResolution;
            start = defaultStart;
            end = defaultEnd;
            this.function = function;
        }

        public FunctionGrid(Func<float, float, float> function)
        {
            resolution = defaultResolution;
            start = defaultStart;
            end = defaultEnd;
            this.function = (x, z) => new Vector3(x, function(x, z), z);
        }

        public IGrid ChangeResolution(Vector2Int newResolution)
        {
            if (newResolution == resolution)
                return this;

            return new FunctionGrid(function) {
                resolution = newResolution,
                start = start,
                end = end,
            };
        }

        public IGrid SmoothCut(Vector2 croppedResolution, bool fromOrigin)
        {
            return ToDiscrete().SmoothCut(croppedResolution, fromOrigin);
        }

        public DiscreteGrid ToDiscrete()
        {
            return new DiscreteGrid(points);
        }

        private Vector3[,] CalculatePoints()
        {
            var result = new Vector3[resolution.x + 1, resolution.y + 1];
            var step = (end - start) / resolution;
            var point = start;

            for (var x = 0; x <= resolution.x; x++) {
                for (var z = 0; z <= resolution.y; z++) {
                    result[x, z] = function(step.x * x, step.y * z);
                    point += step;
                }
            }

            return result;
        }
    }
}
