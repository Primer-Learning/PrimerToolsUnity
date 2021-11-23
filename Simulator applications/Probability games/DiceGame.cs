using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceGame : MonoBehaviour
{
    internal RollerGroup rollerGroup = null;
    internal CoinBucket coinBucket = null;
    internal int entryFee = 1;
    internal int payout = 7;
    internal List<int> resultsSoFar = new List<int>();
    static float defaultWaitTime = 0;
    float waitTime = defaultWaitTime;
    float payoutWaitTime = 1;

    internal int numPlays = 0;
    internal int numCompleted = 0;
    int totalEntries = 0;
    internal bool isPlaying = false;
    List<DieRoller> busyRollers = new List<DieRoller>();

    void Start() {
        for (int i = 0; i <= 6; i++)
        {
            resultsSoFar.Add(0);
        }
    }

    internal bool IsPlaying() {
        if (numCompleted == totalEntries) { return false; }
        return true;
    }

    internal void Play(int numEntries = 1) {
        isPlaying = true;
        rollerGroup.BeginUpdating();
        totalEntries += numEntries;
        StartCoroutine(play());
    }

    IEnumerator play() {
        while (numCompleted < totalEntries) {
            // Stop the whole thing once the correct number of rolls have been completed
            // But only actually do another roll if there aren't enough other rolls happening.
            if (numPlays < totalEntries) {
                // Find available rollers
                List<DieRoller> idleRollers = rollerGroup.rollers.Where(x => !busyRollers.Contains(x)).ToList();
                // Kick off singlePlay, or wait for available rolleavailable roller
                if (idleRollers.Count > 0 && coinBucket.coins.Count >= entryFee) {
                    numPlays++;
                    DieRoller roller = idleRollers[Director.sceneRandom.Next(idleRollers.Count)];
                    busyRollers.Add(roller);
                    StartCoroutine(singlePlay(roller));
                }
            }
            yield return null;
        } 
        isPlaying = false;
    }
    IEnumerator singlePlay(DieRoller roller) {
        // Pay fee
        coinBucket.PluckCoins(entryFee);
        yield return new WaitForSeconds(1);
        // Roll dice
        yield return roller.singleRoll();
        yield return new WaitForSeconds(waitTime);
        if (Payout()) {
            rollerGroup.rollers[0].source.GetComponent<PrimerCharacter>().animator.SetTrigger("MouthSmile");
            yield return new WaitForSeconds(payoutWaitTime);
            rollerGroup.rollers[0].source.GetComponent<PrimerCharacter>().animator.SetTrigger("MouthClosed");
            yield return new WaitForSeconds(payoutWaitTime);
        }
        numCompleted++;
        busyRollers.Remove(roller);
    }
    
    bool Payout() {
        int wins = rollerGroup.results[6] - resultsSoFar[6];
        coinBucket.AddCoins(wins * payout);
        int invalidRolls = rollerGroup.results[0] - resultsSoFar[0];
        coinBucket.AddCoins(invalidRolls);
        numCompleted -= invalidRolls; //Kinda a hacky place/way to do this, but this makes it so invalid rolls aren't counted. 
        numPlays -= invalidRolls;
        resultsSoFar[6] = rollerGroup.results[6];
        resultsSoFar[0] = rollerGroup.results[0];
        if (wins > 0) { return true; }
        return false;
    }
}
