using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Primer.Simulation
{
    /// <summary>
    ///   Generates a set of points using the Poisson Disc Sampling algorithm.
    ///   Points are kind of random but they have a minimum distance between them.
    ///   https://www.youtube.com/watch?v=7WcmyxyFO7o
    /// </summary>
    public class PoissonDiscSampler
    {
        public static IEnumerable<Vector2> Rectangular(int pointsCount, Vector2 area, float minDistance = 2,
            OverflowMode overflowMode = OverflowMode.None)
        {
            var sampler = new PoissonDiscSampler(minDistance, area, overflowMode);
            sampler.AddPoints(pointsCount);
            return sampler.points;
        }

        public enum OverflowMode
        {
            None,
            // Squeeze mode doesn't seem to properly respect the new min distance
            // Current guess for why is that the new grid boxes not only divide but
            // also shift when the dimensions are odd.
            Squeeze,
            Force,
        }

        private readonly OverflowMode overflowMode;
        private float minDistance;
        private Vector2 sampleRegionSize;
        private float cellSize;
        private int[,] grid;
        private readonly List<Vector2> points = new();
        private List<Vector2> spawnPoints = new();

        private PoissonDiscSampler(
            float minDistance,
            Vector2 sampleRegionSize,
            OverflowMode overflowMode = OverflowMode.None)
        {
            Initialize(minDistance, sampleRegionSize);
            this.overflowMode = overflowMode;
        }

        private void Initialize(float newMinDistance, Vector2 newSampleRegionSize)
        {
            minDistance = newMinDistance;
            sampleRegionSize = newSampleRegionSize;
            cellSize = newMinDistance / Mathf.Sqrt(2);

            grid = new int[
                Mathf.CeilToInt(newSampleRegionSize.x / cellSize),
                Mathf.CeilToInt(newSampleRegionSize.y / cellSize)
            ];

            spawnPoints = new List<Vector2>();

            foreach (var (index, point) in points.WithIndex()) {
                spawnPoints.Add(point);
                SetInGrid(point, index);
            }
        }

        public void AddPoint(int numSamplesBeforeRejection = 30)
        {
            var pointFound = false;

            if (spawnPoints.Count == 0) {
                spawnPoints.Add(sampleRegionSize / 2);
            }

            while (spawnPoints.Count > 0 && !pointFound) {
                var spawnIndex = Random.Range(0, spawnPoints.Count);
                var spawnCentre = spawnPoints[spawnIndex];

                for (var i = 0; i < numSamplesBeforeRejection; i++) {
                    var angle = Random.value * Mathf.PI * 2;
                    var dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    var candidate = spawnCentre + dir * Random.Range(minDistance, 2 * minDistance);

                    if (!IsValidRect(candidate))
                        continue;

                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    SetInGrid(candidate, points.Count);
                    pointFound = true;
                    break;
                }

                if (!pointFound) {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }
        }

        public void AddPoints(int numPoints, int numSamplesBeforeRejection = 30)
        {
            for (var i = 0; i < numPoints; i++) {
                AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
            }

            var storedMinDistance = minDistance;

            // Squeeze mode doesn't seem to properly respect the new min distance
            if (points.Count < numPoints && overflowMode != OverflowMode.None) {
                Initialize(minDistance / 2, sampleRegionSize);

                for (var i = points.Count; i < numPoints; i++) {
                    AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
                }
            }

            if (overflowMode == OverflowMode.Force) {
                while (points.Count < numPoints) {
                    Initialize(minDistance / 2, sampleRegionSize);

                    for (var i = points.Count; i < numPoints; i++) {
                        AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
                    }
                }
            }

            Initialize(storedMinDistance, sampleRegionSize);
        }

        public void AddPointsUntilFull(int numSamplesBeforeRejection = 30)
        {
            if (spawnPoints.Count == 0) {
                spawnPoints.Add(sampleRegionSize / 2);
            }

            while (spawnPoints.Count > 0) {
                AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
            }
        }

        private void SetInGrid(Vector2 point, int index)
        {
            grid[(int)(point.x / cellSize), (int)(point.y / cellSize)] = index;
        }

        private bool IsValidRect(Vector2 candidate)
        {
            var isOutOfBounds = candidate.x < 0
                || candidate.x >= sampleRegionSize.x
                || candidate.y < 0
                || candidate.y >= sampleRegionSize.y;

            if (isOutOfBounds)
                return false;

            var cellX = (int)(candidate.x / cellSize);
            var cellY = (int)(candidate.y / cellSize);
            var searchStartX = Mathf.Max(0, cellX - 2);
            var searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            var searchStartY = Mathf.Max(0, cellY - 2);
            var searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (var x = searchStartX; x <= searchEndX; x++) {
                for (var y = searchStartY; y <= searchEndY; y++) {
                    var pointIndex = grid[x, y] - 1;

                    if (pointIndex == -1)
                        continue;

                    var sqrDst = (candidate - points[pointIndex]).sqrMagnitude;

                    if (sqrDst < minDistance * minDistance) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
