using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public class GenericClip : PrimerClip<GenericBehaviour>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation;
    }
}

