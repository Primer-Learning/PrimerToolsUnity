using System.Collections.Generic;
using Shapes;

namespace Primer.Graph
{
    internal interface ILineBehaviour
    {
        List<PolylinePoint> Points { get; }
    }
}
