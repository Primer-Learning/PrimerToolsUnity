using System;

namespace Primer.Axis
{
    [Serializable]
    public class TicData
    {
        public float value;
        public string label;

        public TicData(float value, int labelOffset) {
            this.value = value;
            label = (value + labelOffset).FormatNumberWithDecimals();
        }
    }
}
