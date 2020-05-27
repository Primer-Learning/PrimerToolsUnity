using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EaseMode {
    Cubic,
    Quadratic,
    SmoothStep, //Built-in Unity function that's mostly linear but smooths edges
    None
}

public static class Helpers
{
    public static float ApplyNormalizedEasing(float t, EaseMode ease) {
        switch (ease) 
        {
            case EaseMode.Cubic:
                return easeInAndOutCubic(0, 1, t);
            case EaseMode.Quadratic:
                return easeInAndOutQuadratic(0, 1, t);
            case EaseMode.SmoothStep:
                return Mathf.SmoothStep(0, 1, t);
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
