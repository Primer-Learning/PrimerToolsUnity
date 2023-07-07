using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class Agent : MonoBehaviour
    {
        private static readonly int scoop = Animator.StringToHash("Scoop");

        private LandscapeWalker movementCache;
        private LandscapeWalker movement => transform.GetOrAddComponent(ref movementCache);

        private PrimerBlob blobCache;
        private PrimerBlob blob => transform.GetOrAddComponent(ref blobCache);

        public float energy;
        public Food goingToEat;

        public bool canSurvive => energy >= 1;
        public bool canReproduce => energy >= 2;

        public async UniTask GoToEat(Food food)
        {
            goingToEat = food;
            var tween = movement.WalkTo(food.transform);
            tween.duration /= 2;
            await tween;
        }

        public async UniTask Eat(Food food)
        {
            goingToEat = null;
            energy++;

            var bite = food.Consume();
            blob.animator.SetTrigger(scoop);
            await bite.MoveBy(y: 0.5f);
            await bite.ShrinkAndDispose();
        }

        public async UniTask ReturnHome(Vector2 position)
        {
            await movement.WalkToLocal(position);

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
