using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Primer.Graph;
using UnityEngine;

namespace Primer.Graph
{
    public class PointController2 : PrimerBehaviour
    {
        public PointAnimation appearance = PointAnimation.ScaleUpFromZero;

        void Start() {
            runAppearanceAnimation();
        }

        async void runAppearanceAnimation() {
            switch (appearance) {
                case PointAnimation.ScaleUpFromZero:
                    await ScaleUpFromZeroAwaitable();
                    break;

                case PointAnimation.TweenYAxis:
                    var target = transform.position;
                    transform.position = new Vector3(target.x, 0, target.z);
                    await ScaleUpFromZeroAwaitable();
                    await Task.Delay(500);
                    await MoveTo(target);
                    break;
            }
        }
    }
}
