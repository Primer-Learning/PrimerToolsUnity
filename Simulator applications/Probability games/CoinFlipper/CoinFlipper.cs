using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public enum PlayerType {
    Fair,
    Cheater,
    Unknown
} 
public enum ResultsDisplayMode {
    Gallery,
    Numeric,
    Individual
}
public class CoinFlipper : Simulator 
{
    /*
    Need a flipperCharacter prefab and a coin prefab
    The flipperCharacter should flip the coin in a similar way to how it rolls a die
    The coin needs a place to land. Probably just a plane under the flipperCharacter. Wood textured.
    After the coin lands, move it to a display area. 
    The display area should be a PrimerObject so it can be moved around.

    Define expected effect size to calculate power-value and then also p-value
    */
    public PlayerType trueType = PlayerType.Fair;
    public PlayerType labeledType = PlayerType.Unknown;
    public PrimerCharacter flipperCharacterPrefab = null;
    [SerializeField] PrimerObject floorPrefab = null;
    public System.Random rng = null;
    
    public ResultsDisplayMode resultsDisplayMode = ResultsDisplayMode.Gallery;
    public PrimerCharacter flipperCharacter = null;
    public PrimerObject coin = null;
    public GameObject coinPrefab = null;
    public float headsRate = 0.5f;
    bool expectedHeads; // For testing whether the bias works properly.
    public PrimerObject floor = null;

    Vector3 flipperCharacterPosition = new Vector3(-2, 0, 2);
    Quaternion flipperCharacterRotation = Quaternion.Euler(0, 135, 0);
    float floorSize = 3;
    Vector3 sourceDisp = new Vector3(0, 0.8f, 0.8f);
    List<float> currentICs = null;
    public float initialHorizontalSpeed = 3;
    public float initialVerticalSpeed = 12;
    public float initialAngularSpeed = 16;
    public float angleDeviation = 30;
    float maxInitialHorizontalSpeed = 4;
    float maxInitialVerticalSpeed = 13;
    float maxInitialAngularSpeed = 40;
    float maxAngleDeviation = 30;
    float minInitialHorizontalSpeed = 1;
    float minInitialVerticalSpeed = 7;
    float minInitialAngularSpeed = 4;
    float minAngleDeviation = -30;

    float tossDelay = 0.5f;
    float maxTime = 4;
    float landCheckPause = 0.5f;
    float stoppedThreshold = 0.01f;
    
    // Recording and displaying results
    public PrimerObject display = null;
    public int displayRowLength = 8;
    public float displayCoinSpacing = 1;
    Vector3 displayPos = new Vector3(0, 2.5f, 1.5f);
    float displayScale = 0.5f;
    public List<PrimerObject> displayCoins = new List<PrimerObject>();
    public PrimerText headsCountDisplay = null;
    public PrimerText tailsCountDisplay = null;
    public Vector3 numericDisplayPosition = Vector3.zero;
    public List<int> results = new List<int>();
    public CoinFlipSimManager manager = null;
    public bool currentlyFlipping = false;
    public bool currentlyInASeriesOfFlips = false;

    public Vector3 individualOffset = new Vector3(0f, 1f, 4f);

    void Start() {
        if (rng == null) {
            int seed = System.Environment.TickCount;
            rng = new System.Random(seed);
            Debug.LogWarning("Creating new seed for an individual flipper");
        }
    }
    public void Appear(float stagger = 0.25f) {
        if (flipperCharacter == null) {
            flipperCharacter = Instantiate(flipperCharacterPrefab);
        }
        flipperCharacter.transform.parent = transform;
        flipperCharacter.transform.localPosition = flipperCharacterPosition;
        flipperCharacter.transform.localRotation = flipperCharacterRotation;
        flipperCharacter.transform.localScale = Vector3.zero;
        floor = Instantiate(floorPrefab);
        floor.transform.parent = transform;
        floor.transform.localPosition = Vector3.zero;
        floor.SetIntrinsicScale(floorSize);

        StartCoroutine(appear(stagger));

        display = new GameObject().MakePrimerObject();
        display.name = "Results display";
        display.transform.parent = transform;
        display.transform.localPosition = displayPos;
        display.transform.localScale = Vector3.one * displayScale;
    }
    IEnumerator appear(float stagger) {
        floor.ScaleUpFromZero();
        yield return new WaitForSeconds(stagger);
        flipperCharacter.ScaleUpFromZero();
    }
    public void FormOfBlob(float duration = 0.5f) {
        floor.ScaleDownToZero(duration: duration);
        flipperCharacter.ScaleTo(1, duration: duration);
        flipperCharacter.MoveTo(Vector3.zero, duration: duration);
        flipperCharacter.RotateTo(Quaternion.Euler(0, 180, 0), duration: duration);
        foreach (PrimerObject c in display.GetComponentsInChildren<PrimerObject>()) {
            c.ScaleDownToZero(duration: duration);
        }
    }
    public void Flip(int outcome = -1, int initialConditionsIndex = -1) {
        StartCoroutine(flip(outcome: outcome, initialConditionsIndex: initialConditionsIndex));
    }
    IEnumerator flip(int outcome = -1, int initialConditionsIndex = -1) {
        SetFlipParameters(initialConditionsIndex: initialConditionsIndex);
        if (flipperCharacter.animator != null) {
            flipperCharacter.animator.SetTrigger("Scoop"); 
        }
        //Spawn coin
        SpawnCoin(outcome);
        yield return new WaitForSeconds(tossDelay);
        TossCoin();
    }
    void SpawnCoin(int outcome = -1) {
        coin = Instantiate(coinPrefab).MakePrimerObject();
        coin.GetComponent<Rigidbody>().velocity = Vector3.zero;
        coin.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        coin.GetComponent<Rigidbody>().isKinematic = true;
        coin.transform.parent = flipperCharacter.transform;
        // coin.transform.localScale = Vector3.one;
        coin.transform.localPosition = sourceDisp;
        int extraRot = 0;
        if (outcome == 1 || (outcome == -1 && rng.NextDouble() < headsRate)) { 
            extraRot = 180; 
            expectedHeads = true;
        }
        else{ expectedHeads = false; }
        coin.transform.localRotation = Quaternion.Euler(-45 + extraRot, 0, rng.Next(360));
        coin.transform.parent = flipperCharacter.transform.FindDeepChild("bone_neck");
        coin.SetIntrinsicScale(0.3f);
        coin.ScaleUpFromZero();
    }
    void TossCoin() {
        coin.transform.parent = null;
        Rigidbody rb = coin.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Quaternion.Euler(0, angleDeviation, 0) * flipperCharacter.transform.forward * initialHorizontalSpeed + Vector3.up * initialVerticalSpeed;    
        rb.maxAngularVelocity = initialAngularSpeed;
        rb.angularVelocity = flipperCharacter.transform.right * initialAngularSpeed;

        // Character starts looking at coin here
        flipperCharacter.StartLookingAt(coin.transform, correctionVector: Vector3.down); 
    }
    public IEnumerator waitUntilCoinIsStopped() {
        Rigidbody co = coin.GetComponent<Rigidbody>();
        // Check that it's still twice, a small delay apart
        // Or just quit if it has been too long. Idk how to reliably have it settle to zero motion.
        float startTime = Time.time;
        int stateIndex = 0;
        while (stateIndex < 2) {
            if (Time.time > startTime + maxTime) {
                Debug.Log("Flip timeout");
                if (manager.savingNewParameters) {
                    manager.sillyInitialConditions.Add(new List<float>(){initialVerticalSpeed, initialHorizontalSpeed, initialAngularSpeed});
                }
                if (manager.refiningOldParameters) {
                    manager.validInitialConditions.Remove(currentICs);
                }
                flipperCharacter.StopLooking(duration: tossDelay - (float) 2 / 60);
                SetFlipParameters();
                yield return reflip();
                break;
            }
            if (stateIndex > 0) {
                yield return new WaitForSeconds(landCheckPause);
            }
            if (co.velocity.sqrMagnitude < stoppedThreshold && co.angularVelocity.sqrMagnitude < stoppedThreshold) {
                stateIndex++;
            }
            else { stateIndex = 0; }
            yield return null;
        }
        if (expectedHeads == true) {
            if (GetResult() != 1) { Debug.Log("Unexpected Tails"); }
        }
        else {
            if (GetResult() != 0) { Debug.Log("Unexpected Heads"); }
        }
        // For testing
        if (manager.savingNewParameters) {
            if (expectedHeads == true) {
                if (GetResult() == 1) {
                    Debug.Log("Heads as expected");
                    manager.validInitialConditions.Add(new List<float>(){initialVerticalSpeed, initialHorizontalSpeed, initialAngularSpeed, angleDeviation});
                }
                else { Debug.Log("Unexpected Tails"); }
            }
            else {
                if (GetResult() == 0) {
                    Debug.Log("Tails as expected");
                    manager.validInitialConditions.Add(new List<float>(){initialVerticalSpeed, initialHorizontalSpeed, initialAngularSpeed, angleDeviation});
                }
                else { Debug.Log("Unexpected Heads"); }
            }
        }
        // For testing
        if (manager.testing) {
            if (expectedHeads == true) {
                if (GetResult() == 1) {
                    Debug.Log("Heads as expected");
                }
                else { Debug.Log("Unexpected Tails"); }
            }
            else {
                if (GetResult() == 0) {
                    Debug.Log("Tails as expected");
                }
                else { Debug.Log("Unexpected Heads"); }
            }
        }
        flipperCharacter.StopLooking(duration: tossDelay - (float) 2 / 60);
    }
    IEnumerator reflip() {
        coin.Disappear();
        yield return new WaitForSeconds(0.5f);
        yield return flip();
        yield return waitUntilCoinIsStopped();
    }
    int GetResult() {
        if (Vector3.Dot(coin.transform.forward, Vector3.up) > 0.99f) { return 1; }
        else { return 0; }
    }
    public IEnumerator recordAndDisplay(float duration = 0.5f, int outcome = -1, float delay = 1.5f) {
        if (outcome == -1) { Debug.Log("Outcome = -1, which is a bad thing"); }
        int res = outcome == -1 ? GetResult() : outcome;
        results.Add(res);
        int numResults = results.Count;
        if (resultsDisplayMode == ResultsDisplayMode.Individual) {
            displayCoins.Add(coin);
            // Put the coin where it goes
            coin.transform.parent = transform;
            coin.GetComponent<Rigidbody>().isKinematic = true;
            // Vector3 front = individualOffset;
            // coin.MoveTo(Camera.main.transform.position + front);
            coin.MoveTo(individualOffset);
            if (res == 1)
            {
                coin.RotateTo(Quaternion.Euler(0, 180, 0));
            }
            else { coin.RotateTo(Quaternion.Euler(0, 0, 180)); }
            yield return new WaitForSeconds(delay);
            coin.transform.parent = display.transform;
            Vector3 dest = new Vector3((numResults - 1) % displayRowLength, -(numResults - 1) / displayRowLength, 0);
            coin.MoveTo(dest * displayCoinSpacing);
            coin.ScaleTo(coin.GetIntrinsicScale());
            yield return new WaitForSeconds(0.5f);
        }
        else if (resultsDisplayMode == ResultsDisplayMode.Gallery) {
            displayCoins.Add(coin);
            // Put the coin where it goes
            coin.GetComponent<Rigidbody>().isKinematic = true;
            
            coin.transform.parent = display.transform;
            Vector3 dest = new Vector3((numResults - 1) % displayRowLength, -(numResults - 1) / displayRowLength, 0);
            coin.MoveTo(dest * displayCoinSpacing);
            coin.ScaleTo(coin.GetIntrinsicScale());
            if (res == 1) {
                coin.RotateTo(Quaternion.Euler(0, 180, 0));
            }
            else { coin.RotateTo(Quaternion.Euler(0, 0, 180)); }
            yield return new WaitForSeconds(0.5f);
        }
        else if (resultsDisplayMode == ResultsDisplayMode.Numeric) {
            coin.GetComponent<Rigidbody>().isKinematic = true;
            coin.transform.parent = transform;
            coin.MoveTo(2 * Vector3.up);
            if (res == 1) {
                coin.RotateTo(Quaternion.Euler(0, 180, 0));
            }
            else { coin.RotateTo(Quaternion.Euler(0, 0, 180)); }
            display.transform.localScale = Vector3.one;
            display.transform.localPosition = numericDisplayPosition;
            yield return new WaitForSeconds(1f);
            coin.Disappear();
            if (headsCountDisplay == null) {
                headsCountDisplay = Instantiate(SceneManager.instance.primerTextPrefab);
                headsCountDisplay.tmpro.alignment = TextAlignmentOptions.Left;
                headsCountDisplay.transform.SetParent(display.transform);
                headsCountDisplay.transform.localPosition = new Vector3(0.75f, 2, 2.5f);
                headsCountDisplay.SetIntrinsicScale(0.5f);
                headsCountDisplay.ScaleUpFromZero();

                tailsCountDisplay = Instantiate(SceneManager.instance.primerTextPrefab);
                tailsCountDisplay.tmpro.alignment = TextAlignmentOptions.Left;
                tailsCountDisplay.transform.SetParent(display.transform);
                tailsCountDisplay.transform.localPosition = new Vector3(0.75f, 1.3f, 2.5f);
                tailsCountDisplay.SetIntrinsicScale(0.5f);
                tailsCountDisplay.ScaleUpFromZero();
            }
            int numHeads = results.Sum();
            headsCountDisplay.tmpro.text = $"Heads: {numHeads}";
            int numTails = results.Count - numHeads;
            tailsCountDisplay.tmpro.text = $"Tails: {numTails}";
        }
    }
    public float CalcProbThisExtremeIfNull() {
        int num = results.Count;
        int numHeads = results.Sum();

        double probabilityAtLeastExtreme = 0;
        for (int i = num; i >= numHeads; i--) {
            double ways = Helpers.Choose(num, i);
            double probOfEach = System.Math.Pow(0.5, num);
            // Debug.Log($"{ways} ways, each with probability {probOfEach}.");
            probabilityAtLeastExtreme += ways * probOfEach;
        }
        return (float) probabilityAtLeastExtreme;
    }
    public float CalcProbThisExtremeIfAlternative(float headsRateGuess = 0.75f) {
        int num = results.Count;
        int numHeads = results.Sum();

        double probabilityAtLeastExtreme = 0;
        for (int i = num; i >= numHeads; i--) {
            probabilityAtLeastExtreme += Helpers.Binomial(num, i, headsRateGuess);
        }
        return (float) probabilityAtLeastExtreme;
    }
    public void FlipAndRecord(int outcome = -1, int repetitions = 1, float delay = 0.5f) {
        StartCoroutine(flipAndRecord(outcome: outcome, repetitions: repetitions, delay: delay));
    }
    public IEnumerator flipAndRecord(int outcome = -1, int repetitions = 1, float delay = 0.5f) {
        currentlyInASeriesOfFlips = true;
        for (int i = 0; i < repetitions; i++) {
            currentlyFlipping = true;
            if (outcome == -1) {
                outcome = 0;
                if (rng.NextDouble() < headsRate) {
                    outcome = 1;
                }
            }
            Flip(outcome);
            yield return waitUntilCoinIsStopped();
            yield return recordAndDisplay(outcome: outcome, delay: delay);
            currentlyFlipping = false;
        }
        currentlyInASeriesOfFlips = false;
    }
    public void NonAnimatedFlip() {
        if (rng.NextDouble() < headsRate) {
            results.Add(1);
        }
        else { results.Add(0); }

        if (resultsDisplayMode == ResultsDisplayMode.Numeric) {
            display.transform.localScale = Vector3.one;
            display.transform.localPosition = Vector3.zero;
            if (headsCountDisplay == null) {
                headsCountDisplay = Instantiate(SceneManager.instance.primerTextPrefab);
                headsCountDisplay.tmpro.alignment = TextAlignmentOptions.Left;
                headsCountDisplay.transform.SetParent(display.transform);
                headsCountDisplay.transform.localPosition = new Vector3(0.75f, 2, 2.5f);
                headsCountDisplay.SetIntrinsicScale(0.5f);
                headsCountDisplay.ScaleUpFromZero();

                tailsCountDisplay = Instantiate(SceneManager.instance.primerTextPrefab);
                tailsCountDisplay.tmpro.alignment = TextAlignmentOptions.Left;
                tailsCountDisplay.transform.SetParent(display.transform);
                tailsCountDisplay.transform.localPosition = new Vector3(0.75f, 1.3f, 2.5f);
                tailsCountDisplay.SetIntrinsicScale(0.5f);
                tailsCountDisplay.ScaleUpFromZero();
            }
            int numHeads = results.Sum();
            headsCountDisplay.tmpro.text = $"Heads: {numHeads}";
            int numTails = results.Count - numHeads;
            tailsCountDisplay.tmpro.text = $"Tails: {numTails}";
        }
    }
    void SetFlipParameters(int initialConditionsIndex = -1) {
        if (manager.savingNewParameters) {
            // Section for generating valid initial conditions
            initialHorizontalSpeed = UnityEngine.Random.Range((float) minInitialHorizontalSpeed, (float) maxInitialHorizontalSpeed);
            initialVerticalSpeed = UnityEngine.Random.Range((float) minInitialVerticalSpeed, (float) maxInitialVerticalSpeed);
            initialAngularSpeed = UnityEngine.Random.Range((float) minInitialAngularSpeed, (float) maxInitialAngularSpeed);
            angleDeviation = UnityEngine.Random.Range((float) minAngleDeviation, (float) maxAngleDeviation);
        }
        else {
            // Section for choosing valid initial conditions
            if (!manager.refiningOldParameters) {
                if (initialConditionsIndex != -1) {manager.icParameterIndex = initialConditionsIndex;}
                currentICs = manager.validInitialConditions[manager.icParameterIndex];
                manager.icParameterIndex = (manager.icParameterIndex + 1) % manager.validInitialConditions.Count;
                // Debug.Log(manager.icParameterIndex);
            }
            else {
                currentICs = manager.validInitialConditions[rng.Next(manager.validInitialConditions.Count)];
            }
            initialVerticalSpeed = currentICs[0];
            initialHorizontalSpeed = currentICs[1];
            initialAngularSpeed = currentICs[2];
            angleDeviation = currentICs[3]; 
        }
    }
}