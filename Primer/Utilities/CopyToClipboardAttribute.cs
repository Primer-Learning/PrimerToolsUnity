using System;
using Sirenix.OdinInspector;

namespace Primer
{
    [IncludeMyAttributes]
    [PropertyOrder(2000)]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class CopyToClipboardAttribute : ShowInInspectorAttribute
    {
        public readonly string label;

        public CopyToClipboardAttribute(string label = null)
        {
            this.label = label;
        }
    }
}
