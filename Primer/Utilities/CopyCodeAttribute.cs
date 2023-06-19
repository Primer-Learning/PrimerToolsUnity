using System;
using Sirenix.OdinInspector;

namespace Primer
{
    public class CopyCodeAttribute : Attribute
    {
        public readonly string label;

        public SdfIconType icon { get; } = SdfIconType.Code;
        public ButtonSizes size { get; } = ButtonSizes.Large;

        public CopyCodeAttribute(string label = "Copy code")
        {
            this.label = label;
        }
    }
}
