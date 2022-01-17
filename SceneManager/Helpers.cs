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

public enum EaseMode {
    Cubic,
    Quadratic,
    CubicIn,
    CubicOut,
    SmoothStep, //Built-in Unity function that's mostly linear but smooths edges
    DoubleSmoothStep,
    SmoothIn,
    SmoothOut,
    None
}

public static class Helpers
{
    // Todo: Path creation?
    // https://youtu.be/n_RHttAaRCke
    public static float ApplyNormalizedEasing(float t, EaseMode ease) {

        switch (ease) 
        {
            case EaseMode.Cubic:
                return easeInAndOutCubic(0, 1, t);
            case EaseMode.Quadratic:
                return easeInAndOutQuadratic(0, 1, t);
            case EaseMode.CubicIn:
                return easeInCubic(0, 1, t);
            case EaseMode.CubicOut:
                return easeOutCubic(0, 1, t);
            case EaseMode.SmoothStep:
                return Mathf.SmoothStep(0, 1, t);
            case EaseMode.DoubleSmoothStep:
                return (Mathf.SmoothStep(0, 1, t) + t) / 2;
            case EaseMode.SmoothIn:
                // Stretch the function and just use first half
                return Mathf.SmoothStep(0, 2, t / 2);
            case EaseMode.SmoothOut:
                // Stretch the function and just use second half
                // return t;
                return Mathf.SmoothStep(0, 2, (t + 1) / 2) - 1;
            case EaseMode.None:
                return t;
            default:
                return t;
        }
    }
    public static float easeInAndOutCubic(float startVal, float endVal, float t)
    {
        //Scale time relative to half duration
        t *= 2;
        //handle different
        if (t <= 0) { return startVal; }
        if(t <= 1)
        {
            //Ease in from startVal to half the overall change
            return (endVal - startVal) / 2 * t * t * t + startVal;
        }
        if (t <= 2)
        {
            t -= 2; //Make t negative to use left side of cubic
            //Ease out from half to end of overall change
            return (endVal - startVal) / 2 * t * t * t + endVal;
        }
        else { return endVal; }
    } 
    public static float easeInCubic(float startVal, float endVal, float t)
    {
        //Ease in from startVal
        return (endVal - startVal) * t * t * t + startVal;
    } 
    public static float easeOutCubic(float startVal, float endVal, float t)
    {   
        t -= 1; //Make t negative to use left side of cubic
        //Ease out from half to end of overall change
        return (endVal - startVal) * t * t * t + endVal;
    } 

    public static float easeInAndOutQuadratic(float startVal, float endVal, float t)
    {
        //Scale time relative to half duration
        t *= 2;
        //handle different
        if (t <= 0) { return startVal; }
        if(t <= 1)
        {
            //Ease in from zero to half the overall change
            return (endVal - startVal) / 2 * t * t + startVal;
        }
        if (t <= 2)
        {
            t -= 2; //Make t negative to use other left side of quadratic
            //Ease out from half to end of overall change
            return -(endVal - startVal) / 2 * t * t + endVal;
        }
        else { return endVal; }
    }
    public static List<Vector3> CalculateGridPositions(int numElements, float spacing, string plane = "xz") {
        int sideLength = (int) Math.Ceiling(Math.Sqrt(numElements));
        return CalculateGridPositions(sideLength, sideLength, spacing, plane: plane);
    }
    public static List<Vector3> CalculateGridPositions(int numRows, int numColumns, float spacing, int gridOriginIndexX = -1, int gridOriginIndexY = -1, string plane = "xz") {
        if (gridOriginIndexX == -1) { gridOriginIndexX = (numColumns - 1) / 2; }
        if (gridOriginIndexY == -1) { gridOriginIndexY = (numRows - 1) / 2; }
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
    // Copyright (c) 2020 Sebastian Lague
    // Adapted from Sebastian Lague's code at https://github.com/SebLague/Poisson-Disc-Sampling

    public static List<Vector2> GeneratePointsPoisson(float radius, float sampleRegionRadius, int numSamplesBeforeRejection = 30, bool centered = true) {
        return GeneratePointsPoisson(radius, new Vector2(sampleRegionRadius * 2, sampleRegionRadius * 2), numSamplesBeforeRejection: numSamplesBeforeRejection, centered: centered, circle: true);
    }
    public static List<Vector2> GeneratePointsPoisson(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30, bool centered = false, bool circle = false) {
		float cellSize = radius/Mathf.Sqrt(2);

		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x/cellSize), Mathf.CeilToInt(sampleRegionSize.y/cellSize)];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize/2);
		while (spawnPoints.Count > 0) {
			int spawnIndex = UnityEngine.Random.Range(0,spawnPoints.Count);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = UnityEngine.Random.value * Mathf.PI * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * UnityEngine.Random.Range(radius, 2*radius);
                bool isValid = false;
                if (!circle) { isValid = IsValidRect(candidate, sampleRegionSize, cellSize, radius, points, grid); }
                else { isValid = IsValidCirc(candidate, sampleRegionSize.x / 2, cellSize, radius, points, grid); }
                if (isValid) {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x/cellSize),(int)(candidate.y/cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}
		}
        if (centered) {
            List<Vector2> tempPoints = points;
            points = new List<Vector2>();
            foreach (Vector2 p in tempPoints) {
                points.Add(new Vector2(p.x - sampleRegionSize.x / 2, p.y - sampleRegionSize.y / 2));
            }
        }
		return points;
	}
	static bool IsValidRect(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
		if (candidate.x >=0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) {
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x,y]-1;
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
						if (sqrDst < radius*radius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
	static bool IsValidCirc(Vector2 candidate, float regionRadius, float cellSize, float radius, List<Vector2> points, int[,] grid) {
		if ((candidate - new Vector2(regionRadius, regionRadius)).magnitude < regionRadius) {
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x,y]-1;
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
						if (sqrDst < radius*radius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
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

    public static int Choose(int m, int n) {
        // ulong numerator = Factorial((ulong)m, (ulong)n);
        // ulong denominator = Factorial((ulong)(m-n));
        // return (int)(numerator / denominator);
        List<int> numeratorFactors = GetPrimeFactorsOfFactorial(m);
        List<int> denominatorFactors = GetPrimeFactorsOfFactorial(n);
        denominatorFactors.AddRange(GetPrimeFactorsOfFactorial(m-n));
        
        foreach (int commonFactor in denominatorFactors) {
            numeratorFactors.Remove(commonFactor);
        }

        return numeratorFactors.Aggregate(1, (acc, val) => acc * val);
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
        output.Add(n);
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
        int ways = Helpers.Choose(numEvents, numOfInterest);
        // Could use log probabilities here to prevent underflow if necessary
        // 64 bits is plenty for now, though
        double probOfEach = System.Math.Pow(probOfInterest, numOfInterest) * System.Math.Pow(1 - probOfInterest, numEvents - numOfInterest);

        return ways * probOfEach;
    }
    public static double MaxBinomialAccuracy(int numEvents, double prob1, double prob2) {
        // Wait, maybe this is garbage. Maybe it's frequentist?
        if (prob1 > prob2) {
            double placeHolder = prob1;
            prob1 = prob2;
            prob2 = placeHolder;
        }
        
        double probMass1 = 0;
        double probMass2 = 0;
        int crossoverPoint = -1;
        for (int i = 0; i <= numEvents; i++) {
            double b1 = Binomial(numEvents, i, prob1);
            double b2 = Binomial(numEvents, i, prob2);
            if (b1 > b2) { probMass1 += b1; }
            else { // Includes the case where they are equal, but since they are equal, who cares! (But it will matter if I add weights corresponding to base rates)
                if (crossoverPoint == -1) { 
                    crossoverPoint = i;
                }
                probMass2 += b2;
            }
        }
        return (probMass1 + probMass2) / 2; // Change this when adjusting for different base rates.
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
