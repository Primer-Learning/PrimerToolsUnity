using UnityEngine;

namespace Primer.Shapes
{
    public static class VectorExtensions
    {
        public static bool IsInteger(this Vector2 self)
        {
            return self.x.IsInteger() && self.y.IsInteger();
        }

        public static Vector2 GetDecimals(this Vector2 self)
        {
            return new Vector2(self.x.GetDecimals(), self.y.GetDecimals());
        }
    }
}
