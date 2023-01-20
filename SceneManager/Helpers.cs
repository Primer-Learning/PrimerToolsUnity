using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
This is where I put code when I don't know where else to put it.
There is a 100% chance this could be better organized.
*/

public enum CoordinateFrame {
    Local,
    Global
}

public static class Helpers
{
    // Todo: Path creation?
    // https://youtu.be/n_RHttAaRCke
    public static List<Vector3> CalculateGridPositions(int numElements, float spacing, string plane = "xz") {
        int sideLength = (int) Math.Ceiling(Math.Sqrt(numElements));
        return CalculateGridPositions(sideLength, sideLength, spacing, plane: plane);
    }
    public static List<Vector3> CalculateGridPositions(int numRows, int numColumns, float spacing, float gridOriginIndexX = -1, float gridOriginIndexY = -1, string plane = "xz") {
        if (gridOriginIndexX == -1) { gridOriginIndexX = (float)(numColumns - 1) / 2; }
        if (gridOriginIndexY == -1) { gridOriginIndexY = (float)(numRows - 1) / 2; }
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < numRows; i++) {
            for (int j = 0; j < numColumns; j++) {
                float z = spacing * (i - (float)gridOriginIndexY);
                float x = spacing * (j - (float)gridOriginIndexX);
                if (plane == "xz") {
                    positions.Add(new Vector3(x, 0, -z));
                }
                else if (plane == "xy") {
                    positions.Add(new Vector3(x, -z, 0));
                }
            }
        }
        return positions;
    }
    public static Vector3 GenerateNonCollidingPositionOnPlane(List<Transform> otherTransforms, float range = 1, float maxDistance = 1, int maxTries = 30) {
        return GenerateNonCollidingPositionOnPlane(otherTransforms, rangeX: range, rangeZ: range, maxDistance: maxDistance, maxTries: maxTries);
    }
    public static Vector3 GenerateNonCollidingPositionOnPlane(List<Transform> otherTransforms, float rangeX = 1, float rangeZ = 1, float maxDistance = 1, int maxTries = 30) {
        bool found = false;
        Vector3 newPos = Vector3.zero;
        int loops = 0;
        while (found == false && loops < maxTries) {
            loops++;
            found = true;
            newPos = new Vector3(UnityEngine.Random.Range(-rangeX, rangeX), 0, UnityEngine.Random.Range(-rangeZ, rangeZ));
            foreach (Transform other in otherTransforms) {
                float distance = (other.localPosition - newPos).sqrMagnitude;
                if (distance < maxDistance * maxDistance) {
                    found = false;
                    // Debug.Log("Too close");
                }
            }
        }
        // if (loops == 30) { Debug.Log("Couldn't find a point"); }
        return newPos;
    }
   
    //I ended up not using this, but seems useful!
    public static Component CopyComponentTo(Component original, GameObject destination)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields(); 
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }
    public static float[] PartitionFloat(float toPartion, int numPartitions) {
        System.Random rand = new System.Random();
        if (Director.instance != null) {rand = Director.sceneRandom;}
        else {Debug.LogError("No Random object provided, and no Director is present for a default.");}
        return PartitionFloat(toPartion, numPartitions, rand);
    }
    public static float[] PartitionFloat(float toPartion, int numPartitions, System.Random rand) {
        float[] randomFloats = new float[numPartitions];
        for (int i = 0; i < numPartitions; i++) { 
            randomFloats[i] = (float)rand.NextDouble();
        }
        float sum = randomFloats.Sum();
        for (int i = 0; i < numPartitions; i++) { 
            randomFloats[i] /= sum; 
            randomFloats[i] *= toPartion; 
        }
        return randomFloats;
    }

    public static void Shuffle<T>(this IList<T> list, System.Random rng) {  
        int n = list.Count;  
        while (n > 1) {  
            n--;
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
    public static void Shuffle<T>(this IList<T> list) {  
        int n = list.Count;  
        if (Director.sceneRandom != null) {
            while (n > 1) {  
                n--;
                int k = Director.sceneRandom.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
        else {
            while (n > 1) {  
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
    public static void RotateRight(IList sequence, int count)
    {
        object tmp = sequence[count-1];
        sequence.RemoveAt(count - 1);
        sequence.Insert(0, tmp);
    }

    public static IEnumerable<IList> Permutate(IList sequence, int count)
    {
        if (count == 1) yield return sequence;
        else
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var perm in Permutate(sequence, count - 1))
                    yield return perm;
                RotateRight(sequence, count);
            }
        }
    }

    public static double BoxMuller(double mean, double stdDev) {
        System.Random rand = new System.Random();
        if (Director.instance != null) {rand = Director.sceneRandom;}
        double u1 = 1.0-rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0-rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                    Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                    mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }

    public static float Median(List<float> source){
        int count = source.Count();
        if(count == 0) {
            Debug.LogError("Trying to find median of empty list");
        }
        source = source.OrderBy(n => n).ToList();

        int midpoint = count / 2;
        if(count % 2 == 0)
            return (source[midpoint - 1] + source[midpoint]) / 2;
        else
            return source[midpoint];
    }

    public static float LowerQuartile(List<float> source){
        int count = source.Count();
        if(count == 0) {
            Debug.LogError("Trying to find lower quartile of empty list");
        }
        source = source.OrderBy(n => n).ToList();

        int midpoint = count / 4; //Hey, close enough
        return source[midpoint];
    }
    public static float UpperQuartile(List<float> source){
        int count = source.Count();
        if(count == 0) {
            Debug.LogError("Trying to find lower quartile of empty list");
        }
        source = source.OrderBy(n => n).ToList();

        int midpoint = 3 * count / 4; //Hey, close enough
        return source[midpoint];
    }
    public static ulong Factorial(ulong high, ulong low = 1) {
        // Improvement routes include
        // - BigInt
        // - logs
        // - (Just in the Choose method) The clever implementation following (n / 1) * ((n-1)/2).... 

        // A factorial with the option to not go all the way down to 1
        // Nice for making Choose not overflow
        ulong result = 1;
        for (ulong i = low + 1; i <= high; i++) {
            ulong prevResult = result;
            result *= i;
            // I don't really know how this error will manifest
            if (result < prevResult) {
                Debug.LogError("Factorial result is too big");
            }
        }
        return result;
    }
    public static int Factorial(int high, int low = 1) {
        if (high < 0 || low < 0) { Debug.LogError("Cannot take factorial of a negative number"); }
        ulong result = Factorial((ulong)high, (ulong)low);
        if (result <= int.MaxValue) {
            return (int)result;
        }
        Debug.LogError("Factorial result is too big. You could try using the ulong version.");
        return -1;
    }

    public static double Choose(int m, int n) { // Double for the extra size. Conceptually, an int.
        if (m < 0 || n < 0) {
            Debug.LogError("Choose method received negative argument");
        }
        // ulong numerator = Factorial((ulong)m, (ulong)n);
        // ulong denominator = Factorial((ulong)(m-n));
        // return (int)(numerator / denominator);
        List<int> numeratorFactors = GetPrimeFactorsOfFactorial(m);
        List<int> denominatorFactors = GetPrimeFactorsOfFactorial(n);
        denominatorFactors.AddRange(GetPrimeFactorsOfFactorial(m-n));
        
        foreach (int commonFactor in denominatorFactors) {
            numeratorFactors.Remove(commonFactor);
        }

        // int toReturn = numeratorFactors.Aggregate(1, (acc, val) => acc * val);
        double toReturn = 1;
        foreach (int primeFactor in numeratorFactors) {
            toReturn = checked(toReturn * primeFactor);
        }
        if (toReturn < 0) {
            Debug.LogError("Negative Ways");
        }
        return toReturn;
    }
    public static List<int> GetPrimeFactors(int n) {
        List<int> output = new List<int>();
        int i = 2;
        while (i * i <= n) {
            if (n % i == 0) {
                output.Add(i);
                n /= i;
            }
            else { i++; }
        }
        if (n > 1) {
            output.Add(n);
        }
        return output;
    }
    public static List<int> GetPrimeFactorsOfFactorial(int x) {
        List<int> output = new List<int>();
        for (int i = 2; i <= x; i++) {
            output.AddRange(GetPrimeFactors(i));       
        }
        return output;
    }
    public static double Binomial(int numEvents, int numOfInterest, double probOfInterest) {
        double ways = Helpers.Choose(numEvents, numOfInterest);
        // Could use log probabilities here to prevent underflow if necessary
        // 64 bits is plenty for now, though
        double probOfEach = System.Math.Pow(probOfInterest, numOfInterest) * System.Math.Pow(1 - probOfInterest, numEvents - numOfInterest);

        return ways * probOfEach;
    }
    public static double BayesProbabilityOfCheater(int numEvents, int numHeads, double cheaterProb, double fairProb = 0.5f, double cheaterBaseRate = 0.5f) {
        double probThisOutcomeIfCheater = Binomial(numEvents, numHeads, cheaterProb);
        double probThisOutcomeIfFair = Binomial(numEvents, numHeads, fairProb);

        double probThisOutcomeOverall = probThisOutcomeIfCheater * cheaterBaseRate + probThisOutcomeIfFair * (1 - cheaterBaseRate);

        return probThisOutcomeIfCheater * cheaterBaseRate / probThisOutcomeOverall;
    }
    public static int GetLeadingDigit(int num) {
        if (num < 0) { Debug.LogError("Didn't write GetLeadingDigit with negatives in mind"); }
        while (num >= 10) {
            num /= 10;
        }
        return num;
    }
    public static int GetLeadingDigit(float num) {
        if (num < 0) { Debug.LogError("Didn't write GetLeadingDigit with negatives in mind"); }
        if (num > 1) { return GetLeadingDigit((int)num); }
        while (num < 1) {
            num *= 10;
        }
        return (int)num;
    }

    public static float ClampPMOne(float val) {
        return Mathf.Clamp(val, -1, 1);
    }

    public static void WriteToBinaryFile<T>(T objectToWrite, string filePath)
    {   
        using (Stream stream = File.Open(filePath, FileMode.Create))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }
    }

    public static T ReadFromBinaryFile<T>(string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            return (T)binaryFormatter.Deserialize(stream);
        }
    }

    public static T LoadFromResources<T>(string fileName) {
        TextAsset textAsset = Resources.Load(fileName) as TextAsset;
        Stream stream = new MemoryStream(textAsset.bytes);
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        return (T)formatter.Deserialize(stream);
    }

    public static IEnumerator CoroutineTimer(IEnumerator aCoroutine) {
        float startTime = Time.time;
        yield return aCoroutine;
        Debug.Log(Time.time - startTime);
    }

    /* This documentation is buried in a thread that I'm tired of tracking down
    public enum TextAlignmentOptions
    {
        TopLeft = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Top,
        Top = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Top,
        TopRight = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Top,
        TopJustified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Top,
        TopFlush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Top,
        TopGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Top,
        Left = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Middle,
        Center = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Middle,
        Right = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Middle,
        Justified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Middle,
        Flush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Middle,
        CenterGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Middle,
        BottomLeft = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Bottom,
        Bottom = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Bottom,
        BottomRight = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Bottom,
        BottomJustified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Bottom,
        BottomFlush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Bottom,
        BottomGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Bottom,
        BaselineLeft = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Baseline,
        Baseline = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Baseline,
        BaselineRight = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Baseline,
        BaselineJustified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Baseline,
        BaselineFlush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Baseline,
        BaselineGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Baseline,
        MidlineLeft = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Geometry,
        Midline = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Geometry,
        MidlineRight = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Geometry,
        MidlineJustified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Geometry,
        MidlineFlush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Geometry,
        MidlineGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Geometry,
        CaplineLeft = _HorizontalAlignmentOptions.Left | _VerticalAlignmentOptions.Capline,
        Capline = _HorizontalAlignmentOptions.Center | _VerticalAlignmentOptions.Capline,
        CaplineRight = _HorizontalAlignmentOptions.Right | _VerticalAlignmentOptions.Capline,
        CaplineJustified = _HorizontalAlignmentOptions.Justified | _VerticalAlignmentOptions.Capline,
        CaplineFlush = _HorizontalAlignmentOptions.Flush | _VerticalAlignmentOptions.Capline,
        CaplineGeoAligned = _HorizontalAlignmentOptions.Geometry | _VerticalAlignmentOptions.Capline
    };
    */
}
public enum PoissonDiscOverflowMode {
    None,
    // Squeeze mode doesn't seem to properly respect the new min distance
    // Current guess for why is that the new grid boxes not only divide but 
    // also shift when the dimensions are odd.
    Squeeze,
    Force
}
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
public static class GameObjectExtension
{
    public static PrimerObject MakePrimerObject(this GameObject go) {
        
        PrimerObject po = go.GetComponent<PrimerObject>();
        if (po != null) { return po; }
        return go.AddComponent<PrimerObject>();
    }
}
public static class TransformExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(aParent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == aName)
                return c;
            foreach(Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }    

    public static void CopyTransform(this Transform t, Transform tToCopy) {
        t.parent = tToCopy.parent;
        t.localPosition = tToCopy.localPosition;
        t.localRotation = tToCopy.localRotation;
        t.localScale = tToCopy.localScale;
    }
    /*
    //Depth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        foreach(Transform child in aParent)
        {
            if(child.name == aName )
                return child;
            var result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
    */
}

public static class StandardShaderUtils
 {
     public enum BlendMode
     {
         Opaque,
         Cutout,
         Fade,
         Transparent
     }
 
     public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
     {
         switch (blendMode)
         {
             case BlendMode.Opaque:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = -1;
                 break;
             case BlendMode.Cutout:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                 standardShaderMaterial.SetInt("_ZWrite", 1);
                 standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 2450;
                 break;
             case BlendMode.Fade:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 3000;
                 break;
             case BlendMode.Transparent:
                 standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                 standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                 //standardShaderMaterial.SetInt("_ZWrite", 0);
                 standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                 standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                 standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                 standardShaderMaterial.renderQueue = 2001;
                 break;
         }
 
     }
 }
