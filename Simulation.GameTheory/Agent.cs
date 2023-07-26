using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class Agent : LandscapeWalker
    {
        private static readonly int scoop = Animator.StringToHash("Scoop");
        // private static readonly int mouthOpenWide = Animator.StringToHash("MouthOpenWide");
        // private static readonly int mouthClosed = Animator.StringToHash("MouthClosed");


        private PrimerBlob blobCache;
        private PrimerBlob blob => transform.GetOrAddComponent(ref blobCache);

        public float energy;
        public FruitTree goingToEat;

        public bool canSurvive => energy >= 1;
        public bool canReproduce => energy >= 2;

        public async UniTask GoToEat(FruitTree tree)
        {
            goingToEat = tree;
            var tween = WalkTo(tree.transform);
            tween.duration /= 2;
            await tween;
        }

        public async UniTask Eat(Food food)
        {
            goingToEat = null;
            energy++;
            blob.animator.SetTrigger(scoop);

            var mouthPosition = transform.TransformPoint(Vector3.forward * 0.3f + Vector3.up * 1f);
            var bite = food.Consume();

            await bite.MoveTo(mouthPosition, globalSpace: true);
            await bite.ShrinkAndDispose();
        }

        public async UniTask ReturnHome(Vector2 position)
        {
            await WalkToLocal(position);

            if (this == null)
                return;

            var originalRotation = transform.rotation;
            transform.LookAt(Vector3.zero);
            var targetRotation = transform.rotation;

            if (targetRotation == originalRotation)
                return;

            transform.rotation = originalRotation;
            await transform.RotateTo(targetRotation);
        }

        public void ConsumeEnergy()
        {
            energy = 0;
        }
    }
}
