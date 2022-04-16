using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class CoinFlipSimManager : SimulationManager
{
    internal CoinFlipper flipperPrefab = null;
    internal List<CoinFlipper> flippers = new List<CoinFlipper>();
    // Faking the physics
    public bool savingNewParameters;
    public bool refiningOldParameters;
    public bool testing;
    internal int icParameterIndex = 0;
    string physicsPath;
    string sillyPath;
    internal List<List<float>> validInitialConditions = new List<List<float>>();
    internal List<List<float>> sillyInitialConditions = new List<List<float>>();
    // Lists for categories
    internal List<CoinFlipper> nulls;
    internal List<CoinFlipper> alternatives;
    internal List<CoinFlipper> negatives;
    internal List<CoinFlipper> positives;

    internal List<CoinFlipper> trueNegatives;
    internal List<CoinFlipper> truePositives;
    internal List<CoinFlipper> falseNegatives;
    internal List<CoinFlipper> falsePositives;

    internal void Initialize(CoinFlipper flipperPrefabArg = null) {
        // SetUpPhysicsPath();
        if (!savingNewParameters) {
            UnpackInitialConditions();
            // try { UnpackInitialConditions(); }
            // catch { savingNewParameters = true; }
        }
        if (flipperPrefabArg == null) {
            flipperPrefabArg = Resources.Load<CoinFlipper>("FlipperPrefab");
        }
        flipperPrefab = flipperPrefabArg;
    }
    internal void WrapUp() {
        if (savingNewParameters || refiningOldParameters) {
            SavePhysicsFiles();
        }
    }
    void UnpackInitialConditions() {
        validInitialConditions = Helpers.LoadFromResources<List<List<float>>>("initialConditions");
        sillyInitialConditions = Helpers.LoadFromResources<List<List<float>>>("sillyConditions");
    }
    void SetUpPhysicsPath() {
        string path = Application.dataPath;

        // Todo: Figure out how to make this work if someone changed the directory name away from
        // "PrimerTools". Application.dataPath goes to the Assets folder.
        path = Path.Combine(path, "PrimerTools", "Simulator applications", "Probability games", "CoinFlipper", "coinFlipInitialConditions");
        path = Path.Combine(path, "Resources");
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        physicsPath = Path.Combine(path, "initialConditions.bytes");
        sillyPath = Path.Combine(path, "sillyConditions.bytes");
    }
    void SavePhysicsFiles() {
        Helpers.WriteToBinaryFile(validInitialConditions, physicsPath);
        Helpers.WriteToBinaryFile(sillyInitialConditions, sillyPath);
    }
    internal IEnumerator testFlipper(int numFlips, int minNumHeads, CoinFlipper f) {
        yield return f.flipAndRecord(repetitions: numFlips);
        f.labeledType = PlayerType.Fair;
        if (f.results.Sum() >= minNumHeads) {
            f.labeledType = PlayerType.Cheater;
        }
    }
    internal IEnumerator testFlippers(int numFlips, int minNumHeads, List<CoinFlipper> these = null) {
        if (these == null) { these = flippers; }
        List<Coroutine> crs = new List<Coroutine>();
        foreach (CoinFlipper f in these) {
            crs.Add(StartCoroutine(testFlipper(numFlips, minNumHeads, f)));
        }
        // This part used to yield on all the coroutines above, but that made Unity crash a lot, so now we track bools.
        bool finished = false;
        while (!finished) {
            finished = true;
            foreach (CoinFlipper f in these) {
                if (f.currentlyInASeriesOfFlips) {
                    finished = false;
                    break;
                }
            }
            yield return null;
        }
        
        CategorizeTestedFlippers();
        Debug.Log($"True Positive Rate = {(float)truePositives.Count / alternatives.Count}");
        Debug.Log($"True Negative Rate = {(float)trueNegatives.Count / nulls.Count}");
    }
    internal void CategorizeTestedFlippers() {
        // Start over each time. Keeps things clean, and it's not that slow.
        nulls = new List<CoinFlipper>();
        alternatives = new List<CoinFlipper>();
        negatives = new List<CoinFlipper>();
        positives = new List<CoinFlipper>();

        trueNegatives = new List<CoinFlipper>();
        truePositives = new List<CoinFlipper>();
        falseNegatives = new List<CoinFlipper>();
        falsePositives = new List<CoinFlipper>();

        foreach (CoinFlipper f in flippers) {
            if (f.trueType == PlayerType.Fair) {
                nulls.Add(f);
                if (f.labeledType == PlayerType.Fair) {
                    negatives.Add(f);
                    trueNegatives.Add(f);
                }
                else {
                    positives.Add(f);
                    falsePositives.Add(f);
                }
            }
            else {
                alternatives.Add(f);
                if (f.labeledType == PlayerType.Fair) {
                    negatives.Add(f);
                    falseNegatives.Add(f);
                }
                else {
                    positives.Add(f);
                    truePositives.Add(f);
                }
            }
        }
    }
    internal void ParallelFlipRuns(int numFlips = 1, List<CoinFlipper> these = null) {
        if (these == null) { these = flippers; }
        foreach (CoinFlipper f in these) {
            f.FlipAndRecord(repetitions: numFlips);
        }
    }
    internal CoinFlipper AddFlipper(float headsRate = 0.5f) {
        CoinFlipper f = Instantiate(flipperPrefab);
        f.headsRate = headsRate;
        f.rng = simRNG;
        f.manager = this;
        flippers.Add(f);
        if (headsRate == 0.5f) { f.trueType = PlayerType.Fair; }
        else { f.trueType = PlayerType.Cheater; }
        return f;
    }
    internal CoinFlipper AddFlipper(float cheaterProbability, float cheaterHeadsRate) {
        if (simRNG.NextDouble() < cheaterProbability) {
            return AddFlipper(cheaterHeadsRate);
        }
        return AddFlipper(0.5f);
    }
    internal void AddFlippers(int num, float headsRate = 0.5f) {
        for (int i = 0; i < num; i++) { AddFlipper(headsRate: headsRate); }
    }
    internal void ShowFlippers() {
        ShowFlippers(skip: 0, stagger: 0);
    }
    internal void ShowFlippers(int skip = 0, float stagger = 0) {
        StartCoroutine(showFlippers(skip, stagger));
    }
    internal void ShowFlippers(List<CoinFlipper> skip = null, float stagger = 0) {
        StartCoroutine(showFlippers(skip, stagger));
    }
    internal void ShowFlippers(CoinFlipper skip = null, float stagger = 0) {
        List<CoinFlipper> skipList = new List<CoinFlipper>(){skip};
        StartCoroutine(showFlippers(skipList, stagger));
    }
    IEnumerator showFlippers(int skip, float stagger) {
        for (int i = skip; i < flippers.Count; i++) {
            flippers[i].Appear();
            if (stagger > 0) { yield return new WaitForSeconds(stagger); }
        }
        yield return null;
    }
    IEnumerator showFlippers(List<CoinFlipper> skip, float stagger) {
        for (int i = 0; i < flippers.Count; i++) {
            if (!skip.Contains(flippers[i])) {
                flippers[i].Appear();
            }
            if (stagger > 0) { yield return new WaitForSeconds(stagger); }
        }
        yield return null;
    }
    internal delegate bool CascadeDelegateType(Vector3 pos, float startTime);
    internal void ShowFlippersCascade(CascadeDelegateType cascadeDelegate, CoinFlipper skip = null) {
        List<CoinFlipper> skipList = new List<CoinFlipper>() {skip};
        StartCoroutine(showFlippersCascade(cascadeDelegate, skipList));
    }
    IEnumerator showFlippersCascade(CascadeDelegateType cascadeDelegate, List<CoinFlipper> skip = null) {
        if (skip == null) {
            skip = new List<CoinFlipper>();
        }
        float startTime = Time.time;
        while (skip.Count < flippers.Count) {
            foreach (CoinFlipper c in flippers) {
                if ( !skip.Contains(c) && cascadeDelegate(c.transform.localPosition, startTime) ) {
                    c.Appear();
                    skip.Add(c);
                }
            }
            yield return null;
        }
    }
    internal void ArrangeAsGrid(int numRows, int numColumns, float spacing = 7, int gridOriginIndexX = -1, int gridOriginIndexY = -1, float duration = 0) {
        List<Vector3> positions = Helpers.CalculateGridPositions(numRows, numColumns, spacing, gridOriginIndexX: gridOriginIndexX, gridOriginIndexY: gridOriginIndexY);
        if (flippers.Count > positions.Count) {
            Debug.LogError($"Only {positions.Count} positions for {flippers.Count} flippers");
        }
        if (duration == 0) {
            for (int i = 0; i < flippers.Count; i++) {
                flippers[i].transform.localPosition = positions[i];
            }
        }
        else {
            for (int i = 0; i < flippers.Count; i++) {
                flippers[i].MoveTo(positions[i], duration: duration);
            }
        }
    }
}