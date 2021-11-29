using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

public class DieRollScene : Director
{
    [Header("Scene parameters")]
    protected List<PrimerText> texts = new List<PrimerText>();
    protected Graph graph;
    PrimerText totalRolls = null;
    PrimerText totalGain = null;
    PrimerText avgGain = null;
    [SerializeField] RollerGroup rollerGroup = null;
    [SerializeField] CoinBucket bucket = null;

    DieRoller mainRoller = null;

    float camAngle = 17;
    DiceGame game = null;

    protected override void Awake() {
        base.Awake();
    }
    protected override void Start() {
        int numTextObjs = 3;
        for (int i = 0; i < numTextObjs; i++) {
            PrimerText nt = Instantiate(textPrefab, camRig.cam.transform);
            nt.tmpro.alignment = TextAlignmentOptions.Left;
            texts.Add(nt);
        }
        totalRolls = texts[0];
        totalGain = texts[1];
        avgGain = texts[2];

        rollerGroup.updateMode = UpdateMode.Interval;
        rollerGroup.onUpdate = UpdateStatsDisplays;
        game = new GameObject().AddComponent<DiceGame>();
        game.rollerGroup = rollerGroup;
        game.coinBucket = bucket;

        camRig.cam.transform.localPosition = new Vector3(0, 0, -4);
        camRig.transform.localPosition = new Vector3(0, 0, 0);
        camRig.transform.localRotation = Quaternion.Euler(camAngle, 0, 0);

        base.Start();
    }

    //Define event actions
    IEnumerator StartGame() {
        SetUpGraph();
        bucket.SetIntrinsicScale();
        bucket.transform.localScale = Vector3.zero;

        mainRoller = rollerGroup.AddRoller();
        mainRoller.transform.localPosition = new Vector3(-1.4f, -1, 0);
        mainRoller.transform.localScale = Vector3.one * 0.15f;
        bucket.transform.localPosition = new Vector3(1.4f, -1, 0);  

        mainRoller.AnimateIn();
        yield return new WaitForSeconds(1);
        bucket.ScaleUpFromZero();
        yield return new WaitForSeconds(1);
        bucket.AddCoins(10);
        yield return new WaitForSeconds(2);
        camRig.RotateTo(Quaternion.Euler(75, 0, 0), duration: 2);
        yield return new WaitForSeconds(2);
        game.Play(numEntries: 5000);
        
        yield return new WaitForSeconds(8f);

        camRig.RotateTo(Quaternion.Euler(17, 0, 0));
        camRig.MoveTo(0.5f * Vector3.up);
        // mainRoller.MoveTo(new Vector3(-2.5f, -1.5f, 0));
        // mainRoller.ScaleTo(0.1f);
        // bucket.MoveTo(new Vector3(-0.75f, -1.5f, 0));
        // bucket.ScaleTo(0.1f);
        graph.transform.localPosition = new Vector3(-2.25f, 0.7f, 2);
        graph.ScaleTo(0.65f);
        yield return new WaitForSeconds(1);

        // FYI, this is not a great way to handle text
        totalGain.transform.localPosition = new Vector3(5, 11, 30);
        totalGain.ScaleUpFromZero();
        yield return new WaitForSeconds(0.1f);
        totalRolls.transform.localPosition = new Vector3(5, 8.5f, 30);
        totalRolls.ScaleUpFromZero();
        yield return new WaitForSeconds(0.1f);
        avgGain.transform.localPosition = new Vector3(5, 6, 30);
        avgGain.ScaleUpFromZero();

        yield return new WaitForSeconds(1);
        mainRoller.tray.FadeOut();
        bucket.FadeOut();

        // Add a bunch of them
        PrimerObject group = new GameObject("Group container").AddComponent<PrimerObject>();
        group.transform.position = rollerGroup.rollers[0].transform.position;
        rollerGroup.rollers[0].transform.parent = group.transform;

        yield return new WaitForSeconds(2);
        group.ScaleTo(Vector3.one * 0.15f);
        group.MoveTo(new Vector3(-1.4f, -0.25f, -0.8f));
        yield return AddRollerGridSpherical(5, 5, 5, group, 4, fade: true);
        yield return Accelerate(5, 3);

        while (game.IsPlaying()) {
            yield return null;
        }
        Time.timeScale = 1;
        
        yield return new WaitForSeconds(2);
        List<PrimerObject> toDisappear = new List<PrimerObject>() {
            graph, group, totalGain, totalRolls, avgGain, bucket 
        };
        foreach (PrimerObject p in toDisappear) {
            p.ScaleDownToZero();
            yield return new WaitForSeconds(0.2f);
        }
    }

    void SetUpGraph() {
        graph = Instantiate(graphPrefab);
        graph.transform.localPosition = new Vector3(-3, -1, -1);
        graph.transform.localRotation = Quaternion.Euler(17, 0, 0); 
        Dictionary<float, string> xTics = new Dictionary<float, string> () {
            {1, 1.ToString()},
            {2, 2.ToString()},
            {3, 3.ToString()},
            {4, 4.ToString()},
            {5, 5.ToString()},
            {6, 6.ToString()}
        };
        graph.Initialize(
            xTicStep: 1,
            yTicStep: 2f,
            zHidden: true,
            xMax: 7,
            manualTicMode: true,
            manualTicsX: xTics,
            yMax: 10,
            xAxisLength: 3f,
            yAxisLength: 3f,
            scale: 1f,
            arrows: "neither",
            xAxisLabelPos: "along",
            xAxisLabelString: "Result",
            yAxisLabelString: "Count"
        );
        List<Color> colors = new List<Color>() {
            PrimerColor.Purple,
            PrimerColor.Blue,
            PrimerColor.Green,
            PrimerColor.Yellow,
            PrimerColor.Orange,
            PrimerColor.Red
        };
        rollerGroup.colorList = colors;
        BarDataManager bd = graph.AddBarData();
        graph.barData.SetColors(colors);
        // graph.transform.parent = camRig.cam.transform;
        graph.transform.localScale = Vector3.zero;
        // graph.ScaleUpFromZero();
        rollerGroup.SetUpGraph(graph);
        graph.SetIntrinsicScale();
        graph.transform.localScale = Vector3.zero;
    }
    void UpdateStatsDisplays() {
        int rollCount = rollerGroup.results.Skip(1).Sum();
        int gain = rollerGroup.results[6] * game.payout - game.numCompleted * game.entryFee;
        totalGain.tmpro.text = $"Total gain: {gain}";
        totalRolls.tmpro.text = $"Total rolls: {rollCount}";
        avgGain.tmpro.text = $"Average gain: " + ( (float) gain / rollCount ).ToString("0.00");
    }

    IEnumerator Accelerate(float target, float realtimeDuration) {
        int beginFrame = Time.frameCount;
        // Debug.Log("Accel begin frame: " + Time.frameCount.ToString());
        // Realtime duration in seconds assumes 60 fps
        // target = current * rate ^ ( 60 * realtimeDuration )
        float rate = Mathf.Pow(2, ( Mathf.Log(target / Time.timeScale, 2) / ( 60 * realtimeDuration ) ) );
        if (rate > 1) {
            while (Time.timeScale < target) {
                Time.timeScale *= rate;
                yield return null;
            }
        }
        if (rate < 1) {
            while (Time.timeScale > target) {
                Time.timeScale *= rate;
                yield return null;
            }
        }
        Time.timeScale = target;
        Debug.Log($"Accel frame duration was {Time.frameCount - beginFrame}, should be {60 * realtimeDuration}");
    }

    IEnumerator AddRollerGridSpherical(int width, int height, int depth, PrimerObject group, float duration, bool fade = false) {
        Vector3 spacing = new Vector3 (2.7f, 2.7f, 2.7f) * 2 / 3;
        Vector3 dimensions = new Vector3(width, height, depth);

        // Todo? recenter if dimensions go from even to odd or back

        float maxD = 0;
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    Vector3 position = new Vector3((i - (width - 1) / 2) * spacing.x, (j - (height - 1) / 2) * spacing.y, (k - (depth - 1) / 2) * spacing.z);
                    bool alreadyThere = false;
                    foreach (Transform roller in group.transform) {
                        if ( (roller.localPosition) == position) {
                            alreadyThere = true;
                            break;
                        }
                    }
                    if (!alreadyThere) {
                        positions.Add(position);
                        float D = position.magnitude;
                        if (D > maxD) { maxD = D; }
                    }
                }
            }
        }
        float d = 0;
        maxD += 0.5f;
        float startTime = Time.time;
        while (d < maxD) {
            d = maxD * Helpers.ApplyNormalizedEasing( (Time.time - startTime) / duration , EaseMode.Cubic);
            List<Vector3> newPositions = positions.Where(x => x.magnitude <= d).ToList();
            foreach (Vector3 pos in newPositions)
            {
                DieRoller roller = rollerGroup.AddRoller();
                roller.transform.parent = group.transform;
                roller.transform.localPosition = pos;
                roller.transform.localRotation = Quaternion.identity;
                roller.transform.localScale = Vector3.one * 0.10f;
                roller.AnimateIn(duration: 0.5f);
                if (fade) {
                    roller.tray.FadeOut(delay: 1);
                }
                positions.Remove(pos);
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
    }

    //Construct schedule
    protected override void DefineSchedule() {
        new SceneBlock(1, StartGame);
        // new SceneBlock(2f, Go);
        // new SceneBlock(3f, acceltest);
    }
}
