using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Scenes.Intro_Scene_Sources
{
    [HelpURL("https://www.notion.so/primer-learning/Mixed-and-conditional-strategies-working-draft-688dea7fb1fa424997f409fbc5fb5581?pvs=4#e35f4eb001f24b06a8c6bc8ef82abcac")]
    public class TweenSeriesSequence : Sequence
    {
        public Transform testObject1;
        public Transform testObject2;

        private float leftMagnitude => 3;

        public override void Cleanup()
        {
            base.Cleanup();
            testObject1.transform.localScale = Vector3.zero;
            testObject1.transform.localPosition = Vector3.zero;
            
            testObject2.transform.localScale = Vector3.zero;
            testObject2.transform.localPosition = new Vector3(-leftMagnitude, -1, 0);
        }

        public override async IAsyncEnumerator<Tween> Define()
        {
            
            yield return testObject1.ScaleTo(1);

            yield return Tween.Series(
                testObject1.MoveTo(Vector3.right),
                testObject1.MoveTo(Vector3.up * 1)
            );
            
            yield return Tween.Series(
                testObject1.ScaleTo(Vector3.one * 2),
                testObject1.ScaleTo(Vector3.one)
            );

            yield return testObject1.MoveToDynamic(
                () => Vector3.zero,
                () => testObject1.localPosition.magnitude
            );

            yield return testObject1.MoveTo(new Vector3(-leftMagnitude, 1, 0));
            yield return testObject2.ScaleTo(1);

            // Compound tweens with dynamic constituent tweens
            yield return Tween.Parallel(
                testObject1.MoveToDynamic(
                        () => new Vector3(leftMagnitude, 1, 0),
                        () => leftMagnitude - testObject1.localPosition.x
                    ) with
                    {
                        easing = new EaseWithAccelerationPeriod(0.5f)
                    },
                testObject2.MoveBy(Vector3.right * leftMagnitude) with
                {
                    duration = leftMagnitude, easing = new EaseWithAccelerationPeriod(0.5f)
                }
            );

            yield return Tween.Series(
                testObject1.MoveToDynamic(
                    () => new Vector3(-leftMagnitude, 1, 0),
                    () => testObject1.localPosition.x + leftMagnitude
                ),
                testObject1.MoveToDynamic(
                    () => new Vector3(leftMagnitude, 1, 0),
                    () => -testObject1.localPosition.x + leftMagnitude
                )
            );
        }
    }
}
