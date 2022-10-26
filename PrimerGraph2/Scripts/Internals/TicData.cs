using System;

namespace Primer.Graph
{
    [Serializable]
    public class TicData
    {
        public float value;
        public string label;

        public TicData(float value) {
            this.value = value;
            label = Presentation.FormatNumberWithDecimals(value);
        }
    }
}
