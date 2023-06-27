using Cysharp.Threading.Tasks;
using Primer;
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
            return movement.WalkTo(food.transform).Play();
        }

        public UniTask Eat()
        {
            energy += 1;
            return UniTask.CompletedTask;
        }

        public UniTask ReturnHome(Vector2 position)
        {
            return movement.WalkTo(position).Play();
        }

        public void ConsumeEnergy()
        {
            energy = 0;
        }
    }
}
