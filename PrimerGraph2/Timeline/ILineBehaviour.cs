using System.Collections.Generic;
using Shapes;

namespace Primer.Graph
{
    public interface ILineBehaviour
    {
        List<PolylinePoint> points { get; }
    }
}
