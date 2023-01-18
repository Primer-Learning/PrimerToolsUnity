using System;

namespace Primer.Axis
{
    [Serializable]
    public class TicData
    {
        public float value;
        public string label;

        public TicData(float value) {
            this.value = value;
            label = value.FormatNumberWithDecimals();
        }
    }
}
