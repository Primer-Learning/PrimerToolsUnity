using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;

namespace Scenes.Intro_Scene_Sources
{
    [HelpURL("https://www.notion.so/primer-learning/Mixed-and-conditional-strategies-working-draft-688dea7fb1fa424997f409fbc5fb5581?pvs=4#e35f4eb001f24b06a8c6bc8ef82abcac")]
    public class TweenSeriesSequence : Sequence
    {
        public Transform testObject;

        public override async IAsyncEnumerator<Tween> Define()
        {
            testObject.transform.localScale = Vector3.zero;
            testObject.transform.localPosition = Vector3.zero; 
            
            yield return testObject.ScaleTo(1);

            yield return Tween.Series(
                testObject.MoveTo(Vector3.right),
                testObject.MoveTo(Vector3.up)
            );
            
            yield return Tween.Series(
                testObject.ScaleTo(Vector3.one * 2),
                testObject.ScaleTo(Vector3.one)
            );
        }
    }
}
