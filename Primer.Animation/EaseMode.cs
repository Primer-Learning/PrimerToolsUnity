using System;

namespace Primer.Animation
{
    public enum EaseMode
    {
        Cubic,
        Quadratic,
        CubicIn,
        CubicOut,
        /// <summary>Built-in Unity function that's mostly linear but smooths edges</summary>
        SmoothStep,
        DoubleSmoothStep,
        SmoothIn,
        SmoothOut,
        Custom,
        None,
    }

	public static class EaseModeExtensions
    {
        public static float Apply(this EaseMode ease, float t)
        {
            return ease.GetMethod().Evaluate(t);
        }

        public static IEasing GetMethod(this EaseMode ease)
        {
            // Custom can't be converted to a known IEasing method
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return ease switch {
                EaseMode.None => LinearEasing.instance,
                EaseMode.Cubic => CubicEasing.instance,
                EaseMode.Quadratic => QuadraticEasing.instance,
                EaseMode.CubicIn => CubicInEasing.instance,
                EaseMode.CubicOut => CubicOutEasing.instance,
                EaseMode.SmoothStep => SmoothStepEasing.instance,
                EaseMode.DoubleSmoothStep => DoubleSmoothStepEasing.instance,
                EaseMode.SmoothIn => SmoothInEasing.instance,
                EaseMode.SmoothOut => SmoothOutEasing.instance,
                _ => throw new Exception("Can't automatically get easing method for " + ease),
            };
        }

        public static EaseMode GetModeFor(IEasing method)
        {
            return method switch {
                LinearEasing _ => EaseMode.None,
                CubicEasing _ => EaseMode.Cubic,
                QuadraticEasing _ => EaseMode.Quadratic,
                CubicInEasing _ => EaseMode.CubicIn,
                CubicOutEasing _ => EaseMode.CubicOut,
                SmoothStepEasing _ => EaseMode.SmoothStep,
                DoubleSmoothStepEasing _ => EaseMode.DoubleSmoothStep,
                SmoothInEasing _ => EaseMode.SmoothIn,
                SmoothOutEasing _ => EaseMode.SmoothOut,
                _ => EaseMode.Custom,
            };
        }
    }
}
