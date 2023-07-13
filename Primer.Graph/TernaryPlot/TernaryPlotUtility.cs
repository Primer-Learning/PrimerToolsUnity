using System;
using System.Collections.Generic;
using System.Linq;

namespace Primer.Graph
{
    public static class TernaryPlotUtility
    {
        public static List<float[]> EvenlyDistributedPoints(int steps)
        {
            if (steps < 1)
                throw new ArgumentException("EvenlyDistributedPoints needs at least one step");

            var points = new List<float[]>();

            // We need to iterate over two of the variables. Doesn't matter which.
            // I chose b first because it's the horizontal direction, according to how we draw the graph.
            // It goes from 0 to the max.
            // Also, we're stepping by one to avoid float errors that can give negative numbers and make things weird.
            for (var b = 0; b <= steps; b++) {
                // I chose c next, because it also has a direction according to how the graph is drawn. (a is at the origin)
                // It starts at 0 but stops before the total would get too high
                for (var c = 0; c <= steps - b; c++) {
                    // a is what's left over
                    // And we have to norm
                    points.Add(NormalizedPoint(steps - b - c, b, c));
                }
            }

            return points;
        }

        // This is a static method in this class because "normalize" means different things in different contexts.
        // Seems good to make sure this is always used with that context in mind, rather than making it an array extension.
        public static float[] NormalizedPoint(params float[] point)
        {
            var sum = point.Sum();
            var normalizedPoint = new float[point.Length];

            for (var i = 0; i < point.Length; i++) {
                normalizedPoint[i] = point[i] / sum;
            }

            return normalizedPoint;
        }

        // Keeping this for now
        // manualInitialConditions = new AlleleFrequency<Strategy>(
        //     Random.Range(0f, 1f),
        //     Random.Range(0f, 1f),
        //     Random.Range(0f, 1f)
        // );
        //
        // manualInitialConditions.Normalize();
    }
}
