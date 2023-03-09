using System;
using Primer.Timeline;
using UnityEngine;

namespace Primer.Animation
{
    [Serializable]
    public class RevealChildrenProgressively : Scrubbable
    {
        private Vector3[] originalScales;

        [Space] public EaseMode ease = EaseMode.Cubic;


        public override void Prepare()
        {
            originalScales = SelectChildren((x, i) => x.localScale);
        }

        public void Cleanup()
        {
            ForEachChild((x, i) => x.localScale = originalScales[i]);
            originalScales = null;
        }


        public override void Update(float t)
        {        
            // Execute(0) is now called instead of Cleanup. So calling this cleanup here for now.
            if (t == 0)
            {
                Cleanup();
            }
            for (var i = 0; i < target.childCount; i++) {
                var child = target.GetChild(i);
                var scalingTime = t * (target.childCount + 1);

                child.localScale = Vector3.Lerp(
                    Vector3.zero,
                    originalScales[i],
                    ease.Apply(scalingTime - i)
                );
            }
        }


        // Candidates to become Transform extension methods
        // as those are quite frequent operations
        private void ForEachChild(Action<Transform, int> iterator)
        {
            var childCount = target.childCount;

            for (var i = 0; i < childCount; i++)
                iterator(target.GetChild(i), i);
        }

        private T[] SelectChildren<T>(Func<Transform, int, T> iterator)
        {
            var childCount = target.childCount;
            var result = new T[childCount];

            for (var i = 0; i < childCount; i++)
                result[i] = iterator(target.GetChild(i), i);

            return result;
        }
    }
}
