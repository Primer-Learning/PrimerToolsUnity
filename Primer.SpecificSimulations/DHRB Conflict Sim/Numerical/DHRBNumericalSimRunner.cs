using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Primer;
using Primer.Graph;
using Primer.Simulation;
using Primer.Timeline;
using Shapes;
using Simulation.GameTheory;
using Sirenix.OdinInspector;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

[HelpURL("https://www.desmos.com/calculator/zddbdfhfnc")]
[RequireComponent(typeof(DHRBRewardEditorComponent))]
public class DHRBNumericalSimRunner : MonoBehaviour
{
    public DHRBStrategyManager strategyManager => FindObjectOfType<DHRBStrategyManager>();

    // Self-healing property
    // Ternary Plot can be set from the inspector _but_ if it hasn't been set we just create a new one and use it.
    public TernaryPlot _ternaryPlot;
    private TernaryPlot ternaryPlot => _ternaryPlot == null
        ? _ternaryPlot = new Gnome<TernaryPlot>("TernaryPlot").component
        : _ternaryPlot;

    [FormerlySerializedAs("startOnValueChange")]
    public bool liveUpdate = false;
    public bool nudgeInitialConditionsAwayFromZero = true;

    [Title("Simulation parameters")]
    public float stepSize = 0.1f;
    public int maxIterations = 1000;
    public float minDelta = 0.01f;

    [Title("Line rendering")]
    public float startThickness = 0.01f;
    public float endThickness = 0f;
    public Color startColor = PrimerColor.white;
    public Color endColor = PrimerColor.blue;
    public Material material;

    [Title("Controls")]
    // I wouldn't say I love this name, but it's the best I could come up with.
    // This is the number of steps it takes TernaryPlotUtility.EvenlyDistributedPoints
    // to traverse a side of the triangle.
    public int automaticInitialConditionIncrements = 3;

    private CancellationTokenSource cancellationTokenSource;


    public void OnValidate()
    {
        if (liveUpdate)
            Restart();
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    [Button(ButtonSizes.Large)]
    [HideIf(nameof(cancellationTokenSource))]
    public async void Start()
    {
        if (strategyManager.strategyList.Count == 4) ternaryPlot.isQuaternary = true;
        else if (strategyManager.strategyList.Count == 3) ternaryPlot.isQuaternary = false;
        else Debug.LogError($"The sim runner expects 3 or 4 strategies, but there are {strategyManager.strategyList.Count}");

        cancellationTokenSource = new CancellationTokenSource();
        await PrimerTimeline.RegisterOperation(RunSimulations(cancellationTokenSource.Token));
        cancellationTokenSource = null;
    }

    [Button(ButtonSizes.Large)]
    [ShowIf(nameof(cancellationTokenSource))]
    public void Stop()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = null;
    }

    [Button]
    public void Clear()
    {
        ternaryPlot.Clear();
    }

    private async UniTask RunSimulations(CancellationToken cancellationToken)
    {
        var rewardEditorComponent = GetComponent<DHRBRewardEditorComponent>();

        var sim = new NumericalEvoGameTheorySim<DHRB>(
            rewardEditorComponent.rewardMatrix,
            rewardEditorComponent.baseFitness,
            stepSize
        );

        var container = ternaryPlot.GetContentGnome();
        container.Purge();

        // Create evenly distributed initial conditions and insert the manually defined initial condition.
        var initialConditions = strategyManager.strategyList.Count == 3
            ? TernaryPlotUtility.EvenlyDistributedPoints(
                automaticInitialConditionIncrements,
                nudgeAwayFromZero: nudgeInitialConditionsAwayFromZero
            )
            // There have to be four strategies here since Start checks that its three or four.
            : TernaryPlotUtility.EvenlyDistributedPoints3D(
                automaticInitialConditionIncrements,
                nudgeAwayFromZero: nudgeInitialConditionsAwayFromZero
            );

        // Run all sims
        foreach (var ic in initialConditions) {
            var result = sim.Simulate(strategyManager.AlleleFrequencyFromFloatArray(ic), maxIterations, minDelta);

            // If result is a list and not only a IEnumerable<> that means it's a completed simulation, we don't need to wait
            if (result is not List<AlleleFrequency<DHRB>>)
                await UniTask.Delay(0, cancellationToken: cancellationToken);

            PlotSim(result, container);
        }
    }

    private void PlotSim(IEnumerable<AlleleFrequency<DHRB>> result, Gnome gnome)
    {
        // Use Unity's built-in line renderer to draw the line
        var line = gnome.Add<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = startThickness / 1000;
        line.endWidth = endThickness / 1000;
        line.material = material ? material : new Material(Shader.Find("Sprites/Default"));

        line.colorGradient = new Gradient {
            colorKeys = new[] {
                new GradientColorKey(startColor, 0.0f),
                new GradientColorKey(endColor, 1.0f),
            },
        };

        var pointPositions = new List<Vector3>();
        var totalDistance = 0f;
        Vector3? lastPoint = null;

        // First loop to generate points and calculate total distance
        foreach (var frequencies in result) {
            var newPoint = TernaryPlot.CoordinatesToPosition(strategyManager.ToFloatArray(frequencies));

            if (lastPoint.HasValue)
                totalDistance += Vector3.Distance(lastPoint.Value, newPoint);

            pointPositions.Add(newPoint);
            lastPoint = newPoint;
        }

        // Second loop to set color, thickness based on total distance
        var currentDistance = 0f;
        var points = new List<PolylinePoint>();

        for (var i = 0; i < pointPositions.Count; i++) {
            if (i > 0)
                currentDistance += Vector3.Distance(pointPositions[i], pointPositions[i - 1] );

            var t = totalDistance == 0f ? 0f : currentDistance / totalDistance;
            var color = Color.Lerp(startColor, endColor, t);
            var thickness = Mathf.Lerp(startThickness, endThickness, t);
            points.Add(new PolylinePoint(pointPositions[i], color, thickness));

            line.positionCount = points.Count;
            line.SetPositions(pointPositions.ToArray());

            // await UniTask.Delay(10, cancellationToken: cancellationToken);
        }
    }
}
