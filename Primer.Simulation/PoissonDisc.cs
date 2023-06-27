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
    public class PoissonDisc
    {
        public enum OverflowMode
        {
            None,
            // Squeeze mode doesn't seem to properly respect the new min distance
            // Current guess for why is that the new grid boxes not only divide but
            // also shift when the dimensions are odd.
            Squeeze,
            Force,
        }

        public static IEnumerable<Vector2> Rectangular(
            int points,
            Vector2 size,
            float minDistance = 2,
            OverflowMode overflowMode = OverflowMode.Squeeze)
        {
            var poisson = new PoissonDisc(minDistance, size, circular: false, overflowMode);
            poisson.AddPoints(points);
            return poisson.points;
        }

        private readonly List<Vector2> points;
        public readonly OverflowMode overflowMode;

        private readonly Vector2 sampleRegionSize;
        private readonly bool circular;
        private List<Vector2> spawnPoints;
        private float _minDistance;
        private float cellSize;
        private int[,] grid;

        private float minDistance {
            get => _minDistance;
            set => SetMinDistance(value);
        }

        private PoissonDisc(float minDistance, Vector2 sampleRegionSize, bool circular = false,
            OverflowMode overflowMode = OverflowMode.None)
        {
            // If circular, sampleRegionSize's components must be equal
            this.sampleRegionSize = sampleRegionSize;
            this.circular = circular;
            this.overflowMode = overflowMode;
            _minDistance = minDistance;
            cellSize = minDistance / Mathf.Sqrt(2);
            points = new List<Vector2>();
            Reset();
        }

        public bool AddPoint(int maxSamples = 30, bool handleOverflow = true)
        {
            if (handleOverflow)
                return AddPoints(1) > 0;

            if (spawnPoints.Count == 0)
                spawnPoints.Add(sampleRegionSize / 2);

            while (spawnPoints.Count > 0) {
                var spawnIndex = Random.Range(0, spawnPoints.Count);
                var spawnCenter = spawnPoints[spawnIndex];

                for (var i = 0; i < maxSamples; i++) {
                    var theta = Random.value * Mathf.PI * 2;
                    var dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                    var r = _minDistance * Mathf.Sqrt(Random.value * 0.75f + 0.25f) * 2;
                    var candidate = spawnCenter + dir * r;

                    if (!IsValid(candidate))
                        continue;

                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    return true;
                }

                spawnPoints.RemoveAt(spawnIndex);
            }

            return false;
        }

        public int AddPoints(int numPoints, int maxSamples = 30, bool handleOverflow = true)
        {
            var added = 0;

            for (var i = 0; i < numPoints; i++) {
                if (!AddPoint(maxSamples, handleOverflow: false))
                    break;

                added++;
            }

            if (!handleOverflow || added >= numPoints)
                return added;

            var prevMinDistance = _minDistance;

            switch (overflowMode) {
                case OverflowMode.Squeeze:
                    SetMinDistance(_minDistance / 2);
                    added += AddPoints(numPoints - added, maxSamples);
                    SetMinDistance(prevMinDistance);
                    break;

                case OverflowMode.Force: {
                    SetMinDistance(_minDistance / 2);
                    added += AddPoints(numPoints - added, maxSamples);
                    SetMinDistance(prevMinDistance);

                    if (added < numPoints) {
                        for (var i = 0; i < numPoints - added; i++) {
                            if (circular) {
                                var theta = Random.value * Mathf.PI * 2;
                                var dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                                var r = sampleRegionSize.x / 2 * Mathf.Sqrt(Random.value);
                                points.Add(r * dir);
                            }
                            else {
                                points.Add(
                                    new Vector2(
                                        Random.value * sampleRegionSize.x,
                                        Random.value * sampleRegionSize.y
                                    )
                                );
                            }
                        }
                    }

                    break;
                }

                case OverflowMode.None:
                default:
                    break;
            }

            return added;
        }

        // public void AddPointsUntilFull(int maxSamples = 30)
        // {
        //     AddPoints(int.MaxValue, maxSamples, handleOverflow: false);
        // }

        public bool IsValid(Vector2 candidate)
        {
            var a = !circular || !((candidate - sampleRegionSize / 2).magnitude < sampleRegionSize.x / 2);
            var b = circular
                || candidate.x >= 0
                || candidate.x < sampleRegionSize.x
                || candidate.y >= 0
                || candidate.y < sampleRegionSize.y;

            if (a && b)
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

                    if (sqrDst < _minDistance * _minDistance)
                        return false;
                }
            }

            return true;
        }

        // public List<Vector2> GetCenteredRegionPoints()
        // {
        //     return points.Select(GetCenteredRegionPoint).ToList();
        // }

        // public Vector2 GetCenteredRegionPoint(Vector2 point)
        // {
        //     return point - sampleRegionSize / 2;
        // }

        // public List<Vector2> GetCenteredPoints()
        // {
        //     var offset = points.Bounds().center;
        //     return points.Select(x => x - offset).ToList();
        // }

        private void SetMinDistance(float newMinDistance)
        {
            _minDistance = newMinDistance;
            cellSize = newMinDistance / Mathf.Sqrt(2);
            grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            spawnPoints.Clear();

            for (var i = 0; i < points.Count; i++) {
                var point = points[i];
                spawnPoints.Add(point);
                grid[(int)(point.x / cellSize), (int)(point.y / cellSize)] = i + 1;
            }
        }

        public void Reset()
        {
            grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
            spawnPoints = new List<Vector2>();
        }
    }
}
