using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Timeline;
using UnityEngine;

namespace Simulation.GameTheory
{
    public class Creature : LandscapeWalker
    {
        private static readonly int scoop = Animator.StringToHash("Scoop");
        private static readonly int eatSpeed = Animator.StringToHash("EatSpeed");
        private static readonly int mouthOpen = Animator.StringToHash("MouthOpenWide");
        // private static readonly int mouthOpenWide = Animator.StringToHash("MouthOpenWide");
        // private static readonly int mouthClosed = Animator.StringToHash("MouthClosed");
        public Rng rng;
        [HideInInspector]
        public bool skipAnimations;

        private PrimerBlob blobCache;
        public PrimerBlob blob => transform.GetOrAddComponent(ref blobCache);

        public float energy;
        public FruitTree goingToEat;
        public System.Enum strategy;
        public Home home;

        public SimpleGnome stomach => new ("Stomach", parent: transform);
        
        public bool canSurvive => energy >= 1 || rng.rand.NextDouble() < energy;
        public bool canReproduce => energy >= 2 || rng.rand.NextDouble() < energy - 1;
        
        public Tween GoToEat(FruitTree tree, int fruitIndex = 0)
        {
            goingToEat = tree;
            var tween = WalkTo(tree.fruits[fruitIndex], stopDistance: 1, forcedDuration: skipAnimations ? 0 : -1);
            tween.duration /= 2;
            return tween;
        }

        public Tween EatAnimation(FruitTree tree)
        {
            goingToEat = null;

            var bite = DetachNearestFruit(tree);

            var moveToMouthTween = bite.MoveToDynamic(
                    () => transform.TransformPoint(Vector3.forward * 0.3f + Vector3.up * 1f),
                    () => skipAnimations ? 0 : 0.5f, 
                    globalSpace: true)
                .Observe(onStart: () =>
                {
                    if (!skipAnimations && PrimerTimeline.isPlaying)
                    {
                        blob.animator.SetFloat("EatSpeed", 2);
                        blob.animator.SetTrigger(scoop);
                        blob.Chomp(hold:0);
                    }
                }
            );
            var shrinkTween = bite.ScaleTo(0) with {duration = skipAnimations ? 0 : 0.5f};
            return Tween.Parallel(moveToMouthTween, shrinkTween);
        }

        public Tween ReturnHome(Home home)
        {
            this.home = home;
            // Debug.Log(this.home);
            return WalkTo(home.transform.position);
        }
        
        // public Tween ReturnHome(Vector2 position)
        // {
        //     var walkTween = WalkToLocal(position, forcedDuration: skipAnimations ? 0 : -1);
        //
        //     if (this == null)
        //         return walkTween;
        //
        //     var originalRotation = transform.rotation;
        //     transform.LookAt(Vector3.zero);
        //     var targetRotation = transform.rotation;
        //     
        //     transform.rotation = originalRotation;
        //     var rotateTween = transform.RotateTo(targetRotation) with {duration = skipAnimations ? 0 : 0.5f};
        //     return Tween.Series(walkTween, rotateTween);
        // }

        public void ConsumeEnergy()
        {
            energy = 0;
        }

        private Transform DetachNearestFruit(FruitTree tree)
        {
            // Look at flowers on the tree and return the nearest one
            var distance = float.MaxValue;
            Transform nearestFlowerWithFruit = null;
            foreach (var flower in tree.flowers.Where(x => x.childCount > 0))
            {
                var flowerDistance = Vector3.Distance(transform.position, flower.position);
                if (flowerDistance < distance)
                {
                    distance = flowerDistance;
                    nearestFlowerWithFruit = flower;
                }
            }
            
            if (nearestFlowerWithFruit is null) {
                Debug.LogError("No flower found on tree");
            }

            var fruit = nearestFlowerWithFruit.GetChild(0);
            fruit.SetParent(stomach.transform, worldPositionStays: true);

            return fruit;
        }

        public void PurgeStomach()
        {
            stomach.Reset(hard: true);
        }
    }
}
