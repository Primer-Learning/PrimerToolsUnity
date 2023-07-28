using Simulation.GameTheory;
using Sirenix.OdinInspector;

public class DHRBRewardEditorComponent : RewardEditorComponent<DHRB>
{
    [Title("Base fitness reward")]
    [PropertyOrder(-2)]
    public float baseFitness = 1;
    
    [Title("Strategy match-up rewards")]
    [ShowInInspector]
    [TableMatrix]
    [InfoBox("0 = Dove, 1 = Hawk, 2 = Retaliator, 3 = Bully")]
    [PropertyOrder(-1)]
    // This matrix is transposed compared to the one in PutValuesInMatrix(), because of how TableMatrix works
    public float[,] baseFitnessMatrix {
        get => new [,] {
            { doveVsDove, hawkVsDove, retaliatorVsDove, bullyVsDove },
            { doveVsHawk, hawkVsHawk, retaliatorVsHawk, bullyVsHawk },
            { doveVsRetaliator, hawkVsRetaliator, retaliatorVsRetaliator, bullyVsRetaliator },
            { doveVsBully, hawkVsBully, retaliatorVsBully, bullyVsBully },
        };
        set {
            doveVsDove = value[0, 0];
            doveVsHawk = value[1, 0];
            doveVsRetaliator = value[2, 0];
            doveVsBully = value[3, 0];
            hawkVsDove = value[0, 1];
            hawkVsHawk = value[1, 1];
            hawkVsRetaliator = value[2, 1];
            hawkVsBully = value[3, 1];
            retaliatorVsDove = value[0, 2];
            retaliatorVsHawk = value[1, 2];
            retaliatorVsRetaliator = value[2, 2];
            retaliatorVsBully = value[3, 2];
            bullyVsDove = value[0, 3];
            bullyVsHawk = value[1, 3];
            bullyVsRetaliator = value[2, 3];
            bullyVsBully = value[3, 3];
        }
    }
    
    public float doveVsDove = 1;
    public float doveVsHawk = 0.5f;
    public float doveVsRetaliator = 1;
    public float doveVsBully = 0.5f;

    public float hawkVsDove = 1.5f;
    public float hawkVsHawk = 0;
    public float hawkVsRetaliator = 0;
    public float hawkVsBully = 1.5f;

    public float retaliatorVsDove = 1;
    public float retaliatorVsHawk = 0;
    public float retaliatorVsRetaliator = 1;
    public float retaliatorVsBully = 1.5f;
    
    public float bullyVsDove = 1.5f;
    public float bullyVsHawk = 0.5f;
    public float bullyVsRetaliator = 0.5f;
    public float bullyVsBully = 1;

    [Title("Modifiers")]
    public float demandAmount = 0;
    public float firstStrikeAdvantage = 0;
    public float bluffPenalty = 0;
    
    public override float[,] PutValuesInMatrix()
    {
        return new[,] {
            { doveVsDove, doveVsHawk, doveVsRetaliator - demandAmount, doveVsBully},
            { hawkVsDove, hawkVsHawk, hawkVsRetaliator + firstStrikeAdvantage, hawkVsBully},
            { retaliatorVsDove + demandAmount, retaliatorVsHawk - firstStrikeAdvantage, retaliatorVsRetaliator, retaliatorVsBully},
            { bullyVsDove, bullyVsHawk - bluffPenalty, bullyVsRetaliator - bluffPenalty, bullyVsBully - bluffPenalty}
        };
    }
}
