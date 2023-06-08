using UnityEngine;

namespace Primer.Animation
{
    // We had to separate this interface so IPrimer can merge all the interfaces that inherit this one
    public interface ITransformHolder
    {
        Transform transform { get; }
    }
}
