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
    public float[,] baseFitnessMatrix {
        get => new [,] {
            { doveVsDove, doveVsHawk, doveVsRetaliator, doveVsBully },
            { hawkVsDove, hawkVsHawk, hawkVsRetaliator, hawkVsBully },
            { retaliatorVsDove, retaliatorVsHawk, retaliatorVsRetaliator, retaliatorVsBully },
            { bullyVsDove, bullyVsHawk, bullyVsRetaliator, bullyVsBully },
        };
        set {
            doveVsDove = value[0, 0];
            doveVsHawk = value[0, 1];
            doveVsRetaliator = value[0, 2];
            doveVsBully = value[0, 3];
            hawkVsDove = value[1, 0];
            hawkVsHawk = value[1, 1];
            hawkVsRetaliator = value[1, 2];
            hawkVsBully = value[1, 3];
            retaliatorVsDove = value[2, 0];
            retaliatorVsHawk = value[2, 1];
            retaliatorVsRetaliator = value[2, 2];
            retaliatorVsBully = value[2, 3];
            bullyVsDove = value[3, 0];
            bullyVsHawk = value[3, 1];
            bullyVsRetaliator = value[3, 2];
            bullyVsBully = value[3, 3];
        }
    }
    
    public float doveVsDove = 1;
    public float doveVsHawk = 0;
    public float doveVsRetaliator = 1;
    public float doveVsBully = 0;

    public float hawkVsDove = 2;
    public float hawkVsHawk = -1;
    public float hawkVsRetaliator = -1;
    public float hawkVsBully = 2;

    public float retaliatorVsDove = 1;
    public float retaliatorVsHawk = -1;
    public float retaliatorVsRetaliator = 1;
    public float retaliatorVsBully = 2;
    
    public float bullyVsDove = 2;
    public float bullyVsHawk = 0;
    public float bullyVsRetaliator = 0;
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
