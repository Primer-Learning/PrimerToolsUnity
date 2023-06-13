using System.Collections.Generic;
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
    public static float GetEvenlySpacedFloatGivenTotal(int objectIndex, int totalObjects, float totalSpace)
    {
        // var totalDistance = distanceBetweenObjects * (objects.Count - 1);
        var distanceBetweenObjects = totalSpace / (totalObjects - 1);
        var min = -totalSpace / 2;
        return min + objectIndex * distanceBetweenObjects;
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
