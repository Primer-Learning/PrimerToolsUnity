using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   Generates a set of points using the Poisson Disc Sampling algorithm.
///   Points are kind of random but they have a minimum distance between them.
///   https://www.youtube.com/watch?v=7WcmyxyFO7o
/// </summary>
public class PoissonDiscPointSet
{
    public List<Vector2> points;
    List<Vector2> spawnPoints;
    Vector2 sampleRegionSize;
    bool circular;
    public PoissonDiscOverflowMode overflowMode;
    float minDistance, cellSize;
    int[,] grid;

    public PoissonDiscPointSet(
        float minDistance, Vector2 sampleRegionSize, bool circular = false,
        PoissonDiscOverflowMode overflowMode = PoissonDiscOverflowMode.None
    )
    { // If circular, sampleRegionSize's components must be equal
        this.sampleRegionSize = sampleRegionSize;
        this.circular = circular;
        this.overflowMode = overflowMode;
        this.minDistance = minDistance;
        this.cellSize = minDistance / Mathf.Sqrt(2);
        this.Reset();
    }

    public bool AddPoint(int maxSamples = 30, bool handleOverflow = true)
    {
        if (handleOverflow)
        {
            return AddPoints(1) > 0;
        }
        if (spawnPoints.Count == 0)
        {
            spawnPoints.Add(sampleRegionSize / 2);
        }
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];

            for (int i = 0; i < maxSamples; i++)
            {
                float theta = UnityEngine.Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                float r = minDistance * Mathf.Sqrt(UnityEngine.Random.value * 0.75f + 0.25f) * 2;
                Vector2 candidate = spawnCenter + dir * r;
                if (IsValid(candidate))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    return true;
                }
            }
            spawnPoints.RemoveAt(spawnIndex);
        }
        return false;
    }

    public int AddPoints(int numPoints, int maxSamples = 30, bool handleOverflow = true)
    {
        int added = 0;
        for (int i = 0; i < numPoints; i++)
        {
            if (AddPoint(maxSamples, handleOverflow: false))
                added++;
            else break;
        }

        if (handleOverflow && added < numPoints)
        {
            if (overflowMode == PoissonDiscOverflowMode.Squeeze)
            {
                float prevMinDistance = minDistance;
                SetMinDistance(minDistance / 2);
                added += AddPoints(numPoints - added, maxSamples);
                SetMinDistance(prevMinDistance);
            }
            else if (overflowMode == PoissonDiscOverflowMode.Force)
            {
                float prevMinDistance = minDistance;
                SetMinDistance(minDistance / 2);
                added += AddPoints(numPoints - added, maxSamples);
                SetMinDistance(prevMinDistance);
                if (added < numPoints)
                {
                    for (int i = 0; i < numPoints - added; i++)
                    {
                        if (circular)
                        {
                            float theta = UnityEngine.Random.value * Mathf.PI * 2;
                            Vector2 dir = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
                            float r = sampleRegionSize.x / 2 * Mathf.Sqrt(UnityEngine.Random.value);
                            points.Add(r * dir);
                        }
                        else
                        {
                            points.Add(new Vector2(UnityEngine.Random.value * sampleRegionSize.x, UnityEngine.Random.value * sampleRegionSize.y));
                        }
                    }
                }
            }
        }
        return added;
    }

    public void AddPointsUntilFull(int maxSamples = 30)
    {
        AddPoints(int.MaxValue, maxSamples, handleOverflow: false);
    }

    public bool IsValid(Vector2 candidate)
    {
        if ((circular && (candidate - sampleRegionSize / 2).magnitude < sampleRegionSize.x / 2) ||
            (!circular && candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y))
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < minDistance * minDistance)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    // Get points centered around the center of the region
    public List<Vector2> GetCenteredRegionPoints()
    {
        List<Vector2> centered = new List<Vector2>(points.Count);
        foreach (Vector2 point in points)
        {
            centered.Add(GetCenteredRegionPoint(point));
        }
        return centered;
    }

    public Vector2 GetCenteredRegionPoint(Vector2 point)
    {
        return point - sampleRegionSize / 2;
    }

    // Get points centered based on their location
    public List<Vector2> GetCenteredPoints()
    {
        float minX = float.MaxValue, minY = float.MaxValue, maxX = 0, maxY = 0;
        foreach (Vector2 point in points)
        {
            if (point.x < minX)
                minX = point.x;
            if (point.x > maxX)
                maxX = point.x;
            if (point.y < minY)
                minY = point.y;
            if (point.y > maxY)
                maxY = point.y;
        }
        List<Vector2> centered = new List<Vector2>(points.Count);
        Vector2 offset = new Vector2((minX + maxX) / 2, (minY + maxY) / 2);
        foreach (Vector2 point in points)
        {
            centered.Add(point - offset);
        }
        return centered;
    }

    public void SetMinDistance(float minDistance)
    {
        this.minDistance = minDistance;
        this.cellSize = minDistance / Mathf.Sqrt(2);
        this.grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        this.spawnPoints.Clear();
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            spawnPoints.Add(point);
            grid[(int)(point.x / cellSize), (int)(point.y / cellSize)] = i + 1;
        }
    }

    public void Reset()
    {
        this.grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        this.spawnPoints = new List<Vector2>();
        this.points = new List<Vector2>();
    }
}
