using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Primer.Graph;
using Primer.Simulation;
using Simulation.GameTheory;
using Sirenix.Utilities;
using UnityEngine;

namespace Primer.SpecificSimulations
{
    [RequireComponent(typeof(DHRBRewardEditorComponent))]
    public class DHRBAgentBasedSimultaneousTurnSimController : AgentBasedSimultaneousTurnEvoGameTheorySimController
    {
        public Graph.Graph graph;
        public TernaryPlot ternaryPlot;
        
        protected override void SetStrategyRule()
        {
            simultaneousTurnGameAgentHandler = new DHRBSimultaneousTurnGameAgentHandler();
            simultaneousTurnGameAgentHandler.rewardMatrix = GetComponent<RewardEditorComponent>().rewardMatrix;
        }

        protected override async UniTask OnSimStart()
        {
            var alleles = sim.alleles;
            var numDoves = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Dove));
            var numHawks = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Hawk));
            var numRetaliators = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Retaliator));
            var numBullies = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Bully));
            var numMixed = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.MixedHawkDove));
            
            var total = numDoves + numHawks + numRetaliators + numBullies + numMixed;
            
            if (graph is not null)
            {
                
                using var stackedArea = graph.AddStackedArea("Allele frequencies");
                stackedArea.enabled = true;
                stackedArea.colors =
                    new List<Color>() {PrimerColor.blue, PrimerColor.red, PrimerColor.green, PrimerColor.yellow};
                stackedArea.SetData(
                    new float [] {numDoves},
                    new float [] {numHawks}, 
                    new float [] {numRetaliators},
                    new float [] {numBullies}
                );
                
                await (stackedArea.GrowFromStart() with { duration = skipAnimations ? 0 : 0.5f });
            }

            if (ternaryPlot is not null)
            {
                var gnome = ternaryPlot.GetContentGnome("Agent-based");

                var point = gnome.AddPrimitive(PrimitiveType.Sphere, "population point");
                point.transform.localScale = Vector3.zero;
                var newPosition = TernaryPlot.CoordinatesToPosition(
                    (float)numDoves / total,
                    (float)numHawks / total,
                    (float)numRetaliators / total
                );
                point.transform.localPosition = newPosition;
                await point.transform.ScaleTo(Vector3.one / ternaryPlot.transform.localScale.x);
                
                var line = gnome.Add<LineRenderer>("line");
                line.useWorldSpace = false;
                line.startWidth = 0.001f * ternaryPlot.transform.localScale.x;
                line.endWidth = 0.001f * ternaryPlot.transform.localScale.x;
                line.material = new Material(Shader.Find("Sprites/Default"));
                
                var positions = new [] {newPosition};
                line.positionCount = positions.Length;
                line.SetPositions(positions);
            }
        }

        protected override async UniTask OnCycleCompleted()
        {
            var alleles = sim.alleles;
            var numDoves = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Dove));
            var numHawks = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Hawk));
            var numRetaliators = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Retaliator));
            var numBullies = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.Bully));
            var numMixed = alleles.Count(allele => allele.GetType() == typeof(SimultaneousTurnStrategyGenes.MixedHawkDove));
            
            var total = numDoves + numHawks + numRetaliators + numBullies + numMixed;

            if (graph is not null)
            {
                using var stackedArea = graph.AddStackedArea("Allele frequencies");
                stackedArea.colors =
                    new List<Color>() {PrimerColor.blue, PrimerColor.red, PrimerColor.green, PrimerColor.yellow};
                stackedArea.AddData(numDoves, numHawks, numRetaliators, numBullies);

                await (stackedArea.Transition() with { duration = skipAnimations ? 0 : 0.5f });
            }
            
            if (ternaryPlot is not null)
            {
                var gnome = ternaryPlot.GetContentGnome("Agent-based");

                var point = gnome.AddPrimitive(PrimitiveType.Sphere, "population point");
                point.transform.localScale = Vector3.one / ternaryPlot.transform.localScale.x;

                var newPosition = TernaryPlot.CoordinatesToPosition(
                    (float)numDoves / total,
                    (float)numHawks / total,
                    (float)numRetaliators / total
                );
                
                await (point.transform.MoveTo(newPosition) with { duration = skipAnimations ? 0 : 0.5f });
                
                
                // Use Unity's built-in line renderer to draw the line
                var line = gnome.Add<LineRenderer>("line");
                line.useWorldSpace = false;
                line.startWidth = 0.001f * ternaryPlot.transform.localScale.x;
                line.endWidth = 0.001f * ternaryPlot.transform.localScale.x;
                line.material =  new Material(Shader.Find("Sprites/Default"));
                
                var positions = new Vector3[line.positionCount + 1];
                line.GetPositions(positions);
                positions[^1] = newPosition;
                line.positionCount = positions.Length;
                line.SetPositions(positions);
            }
        }

        // protected override async UniTask OnReset()
        // {
        //     var gnome = ternaryPlot.GetContentGnome("Agent-based");
        //     await gnome.GetChildren().Select(x => x.ScaleTo(0) with { duration = skipAnimations ? 0 : 0.5f }).RunInParallel();
        //     gnome.Purge();
        //     Debug.Log("Reset sim");
        // }
        public Tween PlaceAndScaleTreesAndHomes(int numTrees, int numHomes)
        {
            placer.numberToPlace1 = numTrees;
            placer.numberToPlace2 = numHomes;
            placer.Place();
            trees.ForEach(x => x.transform.localScale = Vector3.zero);
            homes.ForEach(x => x.transform.localScale = Vector3.zero);

            return Tween.Parallel(
                trees.Select(x => x.ScaleTo(1) with { delay = Rng.Range(0.2f) }).RunInParallel(),
                homes.Select(x => x.ScaleTo(1) with { delay = Rng.Range(0.2f) }).RunInParallel()
            );
        }
    }
}