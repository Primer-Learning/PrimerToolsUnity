namespace Primer.Graph
{
    public abstract class Curve
    {
        public float Evaluate(float x) => Evaluate(x, 0);
        public abstract float Evaluate(float x, float z);
    }
}
