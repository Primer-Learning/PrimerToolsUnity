using System;

namespace Primer
{
    [Serializable]
    public class FloatReference
    {
        public bool useConstant = true;
        public float constantValue;
        public FloatVariable variable;

        public float value => useConstant ? constantValue : variable.value;
    }
}
