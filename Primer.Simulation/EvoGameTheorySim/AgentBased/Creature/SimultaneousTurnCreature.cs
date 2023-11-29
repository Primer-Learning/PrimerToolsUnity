using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Animation;
using Primer.Simulation;
using Primer.Simulation.Genome.Strategy;
using Primer.Timeline;
using UnityEngine;
using SimpleGnome = Primer.SimpleGnome;

namespace Simulation.GameTheory
{
    public class SimultaneousTurnCreature : LandscapeWalker
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
        private static Pool mangoPool => Pool.GetPool(CommonPrefabs.Mango);
        
        public float energy;
        public FruitTree goingToEat;
        public SimultaneousTurnGenome strategyGenes = new SimultaneousTurnGenome();
        public SimultaneousTurnAction action => strategyGenes.GetAction(rng);
        public Home home;
        
        public SimpleGnome stomach => new ("Stomach", parent: transform);
        
        public bool canSurvive => energy >= 1 || rng.rand.NextDouble() < energy;
        public bool canReproduce => energy >= 2 || rng.rand.NextDouble() < energy - 1;
        
        public Tween GoToEat(FruitTree tree, int fruitIndex = 0, float forcedDuration = -1, bool high = false)
        {
            goingToEat = tree;

            var fruit = high
                ? tree.highFruits[fruitIndex]
                : tree.fruits[fruitIndex];
            var fruitPos = fruit.GetChild(0).position;
            var offset = (fruitPos.To2D() - tree.transform.position.To2D()).normalized;
            var offset3D = new Vector3(offset.x, 0, offset.y);
            
            var tween = WalkTo(fruit.GetChild(0), offset: offset3D, forcedDuration: forcedDuration);
            return tween;
        }

        public Tween EatAnimation(FruitTree tree, int fruitIndex, float eatDuration = 0.5f, float approachDuration = -1)
        {
            goingToEat = null;
            var fruit = tree.fruits[fruitIndex];
            var actualFruit = fruit.GetChild(0);
            
            // Put it back if it fell off during a tween calculation
            actualFruit.GetOrAddComponent<Rigidbody>().isKinematic = true;
            actualFruit.localPosition = new Vector3(-0.012f, -0.444f, 0.006f);
            actualFruit.localRotation = Quaternion.Euler(-11.966f, 0, -5.68f);
            return Tween.Series(
                GoToEat(tree, fruitIndex, forcedDuration: approachDuration)
                    .Observe(afterComplete: () =>
                {
                    blob.StartLookingAt(actualFruit);
                    tree.DetachFruit(fruit);
                }),
                (Tween.noop with {duration = eatDuration})
                .Observe(afterComplete: () =>
                {
                    blob.Chew(eatDuration);
                    blob.animator.SetBool("ArmsInFront", true);
                }),
                fruit.GetChild(0).ScaleTo(0)
                    .Observe(afterComplete: () =>
                {
                    // mangoPool.Return(fruit);
                    blob.animator.SetBool("ArmsInFront", false);
                    blob.StopLooking(duration: 0.2f);
                }) with { duration = eatDuration }
            );
        }
        public Tween EatAnimationForGroundMango(FruitTree tree, int fruitIndex, float eatDuration = 0.5f, float approachDuration = -1, bool high = false)
        {
            goingToEat = null;
            var fruit= high ? tree.highFruits[fruitIndex] : tree.fruits[fruitIndex];
            var actualFruit = fruit.GetChild(0);
            // // Put it back if it fell off during a tween calculation
            // actualFruit.GetOrAddComponent<Rigidbody>().isKinematic = true;
            // actualFruit.localPosition = new Vector3(-0.012f, -0.444f, 0.006f);
            // actualFruit.localRotation = Quaternion.Euler(-11.966f, 0, -5.68f);
            return Tween.Series(
                GoToEat(tree, fruitIndex, forcedDuration: approachDuration, high: high)
                    .Observe(afterComplete: () =>
                    {
                        blob.StartLookingAt(actualFruit);
                        blob.Chew(eatDuration);
                        blob.animator.SetBool("ArmsInFront", true);
                    }),
                fruit.GetChild(0).ScaleTo(0)
                        .Observe(afterComplete: () =>
                        {
                            // mangoPool.Return(fruit);
                            blob.animator.SetBool("ArmsInFront", false);
                            blob.StopLooking(duration: 0.2f);
                        }) with
                    {
                        duration = eatDuration
                    }
            );
        }

        public Tween ReturnHome(Vector3 offset = default)
        {
            return WalkTo(home.transform, offset: offset);
        }

        public void PurgeStomach()
        {
            stomach.Reset(hard: true);
        }
        public int[] OrderFruitByDistance(FruitTree tree)
        {
            return tree.fruits.OrderBy(x => Vector3.Distance(transform.position, x.position))
                .Select(x => Array.IndexOf(tree.fruits, x)).ToArray();
        }
        public int[] OrderHighFruitByDistance(FruitTree tree)
        {
            return tree.highFruits.OrderBy(x => Vector3.Distance(transform.position, x.position))
                .Select(x => Array.IndexOf(tree.highFruits, x)).ToArray();
        }
    }
}
