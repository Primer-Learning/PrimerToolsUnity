using Primer.Animation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Graph
{
    public partial class Axis
    {
        [FormerlySerializedAs("_thickness")]
        public float thickness = 1;

        private Tween TransitionRod(Transform rodParent)
        {
            var rod = new SimpleGnome("Rod", parent: rodParent);
            var position = new Vector3(rodStart, 0f, 0f);
            var scale = new Vector3(rodEnd - rodStart, thickness, thickness);

            DrawBar(rod);

            return Tween.Parallel(
                rod.transform.localPosition == position ? null : rod.MoveTo(position),
                rod.transform.localScale == scale ? null : rod.ScaleTo(scale)
            );
        }

        private static void DrawBar(SimpleGnome rod)
        {
            var cylinder = rod.Add<Transform>("AxisRod", "AxisRod");
            cylinder.localPosition = new Vector3(0.5f, 0, 0);
            cylinder.localRotation = Quaternion.Euler(0, 0, -90);
            cylinder.localScale = new Vector3(0.0375f, 0.5f, 0.0375f);
        }
    }
}
