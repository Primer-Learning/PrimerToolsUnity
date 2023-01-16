namespace Primer.Animation
{
    public enum EaseMode
    {
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
}
