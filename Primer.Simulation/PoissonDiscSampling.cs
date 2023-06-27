using System.Collections.Generic;
using UnityEngine;

namespace Primer.Simulation
{
    public class PoissonDiscSampling
    {
        public enum PoissonDiscOverflowMode
        {
            None,

            // Squeeze mode doesn't seem to properly respect the new min distance
            // Current guess for why is that the new grid boxes not only divide but
            // also shift when the dimensions are odd.
            Squeeze,
            Force,
        }

        public PoissonDiscOverflowMode overflowMode = PoissonDiscOverflowMode.None;
        float minDistance;
        Vector2 sampleRegionSize;
        bool circular;
        bool centered;
        float cellSize;
        int[,] grid;
        public List<Vector2> points = new List<Vector2>();
        public List<Vector2> Points {
            get {
                Vector2 centerOfMassShift;

                if (centered) {
                    if (!circular) {
                        float maxLeft = Mathf.Infinity;
                        float maxRight = Mathf.Infinity;
                        float maxUp = Mathf.Infinity;
                        float maxDown = Mathf.Infinity;
                        Vector2 total = Vector2.zero;

                        foreach (Vector2 point in points) {
                            total += point;

                            if (sampleRegionSize.x - point.x < maxRight) {
                                maxRight = sampleRegionSize.x - point.x;
                            }

                            if (sampleRegionSize.y - point.y < maxUp) {
                                maxUp = sampleRegionSize.y - point.y;
                            }

                            if (point.x < maxLeft) {
                                maxLeft = point.x;
                            }

                            if (point.y < maxDown) {
                                maxDown = point.y;
                            }
                        }

                        total /= points.Count;
                        centerOfMassShift = sampleRegionSize / 2 - total;

                        if (centerOfMassShift.x > maxRight) {
                            centerOfMassShift.x = maxRight;
                        }

                        if (centerOfMassShift.y > maxUp) {
                            centerOfMassShift.x = maxUp;
                        }

                        if (centerOfMassShift.x < -maxLeft) {
                            centerOfMassShift.x = -maxLeft;
                        }

                        if (centerOfMassShift.y < -maxDown) {
                            centerOfMassShift.y = -maxDown;
                        }

                        List<Vector2> centeredPoints = new List<Vector2>();
                        Vector2 shiftToCenter = centerOfMassShift - sampleRegionSize / 2;

                        foreach (Vector2 point in points) {
                            centeredPoints.Add(point + shiftToCenter);
                        }

                        return centeredPoints;
                    }
                    else {
                        // This shifts the points to the center of the region right away for easier math
                        Vector2 total = Vector2.zero;

                        foreach (Vector2 point in points) {
                            total += point;
                        }

                        total /= points.Count;
                        List<Vector2> centeredPoints = new List<Vector2>();

                        foreach (Vector2 point in points) {
                            centeredPoints.Add(point - total);
                        }

                        Vector2 correction = Vector2.zero;

                        for (int i = 0; i < centeredPoints.Count; i++) {
                            float overhang = centeredPoints[i].magnitude - sampleRegionSize.x / 2;

                            if (overhang > correction.magnitude) {
                                correction = -centeredPoints[i].normalized * overhang;
                            }
                        }

                        for (int i = 0; i < centeredPoints.Count; i++) {
                            centeredPoints[i] += correction;
                        }

                        return centeredPoints;
                    }

                }
                else {
                    return points;
                }
            }
        }
        List<Vector2> spawnPoints = new List<Vector2>();

        public PoissonDiscSampling(float minDistance, Vector2 sampleRegionSize, bool circular = false,
            bool centered = false, PoissonDiscOverflowMode overflowMode = PoissonDiscOverflowMode.None)
        {
            Initialize(minDistance, sampleRegionSize);
            this.circular = circular;
            this.centered = centered;
            this.overflowMode = overflowMode;
        }

        void Initialize(float minDistance, Vector2 sampleRegionSize)
        {
            this.minDistance = minDistance;
            this.sampleRegionSize = sampleRegionSize;
            cellSize = minDistance / Mathf.Sqrt(2);

            grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize),
                Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

            spawnPoints = new List<Vector2>();

            foreach (Vector2 point in points) {
                spawnPoints.Add(point);
                grid[(int)(point.x / cellSize), (int)(point.y / cellSize)] = points.IndexOf(point);
            }
        }

        public void AddPoint(int numSamplesBeforeRejection = 30)
        {
            bool pointFound = false;

            if (spawnPoints.Count == 0) {
                spawnPoints.Add(sampleRegionSize / 2);
            }

            while (spawnPoints.Count > 0 && !pointFound) {
                int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                Vector2 spawnCentre = spawnPoints[spawnIndex];

                for (int i = 0; i < numSamplesBeforeRejection; i++) {
                    float angle = UnityEngine.Random.value * Mathf.PI * 2;
                    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 candidate = spawnCentre + dir * UnityEngine.Random.Range(minDistance, 2 * minDistance);
                    bool isValid = false;

                    if (circular) {
                        isValid = IsValidCirc(candidate);
                    }
                    else {
                        isValid = IsValidRect(candidate);
                    }

                    if (isValid) {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                        pointFound = true;
                        break;
                    }
                }

                if (!pointFound) {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }
        }

        public void AddPoints(int numPoints, int numSamplesBeforeRejection = 30)
        {
            for (int i = 0; i < numPoints; i++) {
                AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
            }

            float storedMinDistance = minDistance;

            // Squeeze mode doesn't seem to properly respect the new min distance
            if (points.Count < numPoints && overflowMode != PoissonDiscOverflowMode.None) {
                Initialize(minDistance / 2, sampleRegionSize);

                for (int i = points.Count; i < numPoints; i++) {
                    AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
                }
            }

            while (points.Count < numPoints && overflowMode == PoissonDiscOverflowMode.Force) {
                Initialize(minDistance / 2, sampleRegionSize);

                for (int i = points.Count; i < numPoints; i++) {
                    AddPoint(numSamplesBeforeRejection: numSamplesBeforeRejection);
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

        bool IsValidRect(Vector2 candidate)
        {
            if (candidate.x >= 0
                && candidate.x < sampleRegionSize.x
                && candidate.y >= 0
                && candidate.y < sampleRegionSize.y) {
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);
                int searchStartX = Mathf.Max(0, cellX - 2);
                int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - 2);
                int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++) {
                    for (int y = searchStartY; y <= searchEndY; y++) {
                        int pointIndex = grid[x, y] - 1;

                        if (pointIndex != -1) {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;

                            if (sqrDst < minDistance * minDistance) {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        bool IsValidCirc(Vector2 candidate)
        {
            if ((candidate - sampleRegionSize / 2).magnitude < sampleRegionSize.x / 2) {
                int cellX = (int)(candidate.x / cellSize);
                int cellY = (int)(candidate.y / cellSize);
                int searchStartX = Mathf.Max(0, cellX - 2);
                int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Mathf.Max(0, cellY - 2);
                int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++) {
                    for (int y = searchStartY; y <= searchEndY; y++) {
                        int pointIndex = grid[x, y] - 1;

                        if (pointIndex != -1) {
                            float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;

                            if (sqrDst < minDistance * minDistance) {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
