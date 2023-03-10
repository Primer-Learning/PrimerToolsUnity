using JetBrains.Annotations;
using Primer;
using UnityEngine;

namespace Primer.Timeline.Tests
{
    [UsedImplicitly]
    public class TestTimelineScrubbable_MoveOrScale : Scrubbable
    {
        private Vector3 initialPosition;
        private Vector3 initialScale;
        public Vector3 moveTo = Vector3.zero;

        public override void Prepare()
        {
            initialPosition = target.localPosition;
            initialScale = target.localScale;
        }

        public override void Cleanup()
        {
            target.localPosition = initialPosition;
            target.localScale = initialScale;
        }

        public override void Update(float t) => Move(t);

        public void Move(float t)
        {
            target.localPosition = Vector3.Lerp(initialPosition, moveTo, t);
        }

        [UsedImplicitly]
        public void Scale(float t)
        {
            target.localScale = Vector3.Lerp(initialScale, moveTo, t);
        }
    }
}
