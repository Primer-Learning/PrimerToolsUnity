using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class CoinFlipSimManager : SimulationManager
{
    [SerializeField] CoinFlipper flipperPrefab = null;
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

    internal void Initialize() {
        SetUpPhysicsPath();
        if (!savingNewParameters) {
            try { UnpackInitialConditions(); }
            catch { savingNewParameters = true; }
        }
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
        // "PrimerUnity". Application.dataPath goes to the Assets folder.
        // This is also a bit long.
        path = Path.Combine(path, "PrimerUnity", "Simulator applications", "Probability games", "CoinFlipper", "coinFlipInitialConditions");
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
        foreach (Coroutine cr in crs) {
            yield return cr;
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
        f.manager = this;
        flippers.Add(f);
        if (headsRate == 0.5f) { f.trueType = PlayerType.Fair; }
        else { f.trueType = PlayerType.Cheater; }
        return f;
    }
    internal CoinFlipper AddFlipper(float cheaterProbability, float cheaterHeadsRate) {
        if (SceneManager.sceneRandom.NextDouble() < cheaterProbability) {
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

    internal void ArrangeAsGrid(int numRows, int numColumns, float spacing = 7, float duration = 0) {
        List<Vector3> positions = Helpers.CalculateGridPositions(numRows, numColumns, spacing);
        if (flippers.Count > positions.Count) {
            Debug.LogError("Not enough positions for all flippers");
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