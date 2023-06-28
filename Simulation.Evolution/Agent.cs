using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using UnityEngine;

namespace Simulation.Evolution
{
    internal class Agent : MonoBehaviour
    {
        private LandscapeWalker movementCache;
        private LandscapeWalker movement => transform.GetOrAddComponent(ref movementCache);

        private float energy;
        public Food goingToEat;

        public bool canSurvive => energy >= 1;
        public bool canReproduce => energy >= 2;

        public UniTask GoToEat(Food food)
        {
            goingToEat = food;
            var animation =  movement.WalkTo(food.transform);
            animation.duration /= 2;
            return animation.Play();
        }

        public UniTask Eat(Food food)
        {
            goingToEat = null;
            energy++;
            // TODO: Play eating animation
            return food.Consume();
        }

        public async UniTask ReturnHome(Vector2 position)
        {
            await movement.WalkToLocal(position).Play();

            var originalRotation = transform.rotation;
            transform.LookAt(Vector3.zero);
            var targetRotation = transform.rotation;

            if (targetRotation == originalRotation)
                return;

            transform.rotation = originalRotation;
            await transform.RotateTo(targetRotation).Play();
        }

        public void ConsumeEnergy()
        {
            energy = 0;
        }
    }
}
