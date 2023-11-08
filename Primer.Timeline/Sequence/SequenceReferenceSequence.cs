using System.Collections.Generic;
using Primer.Animation;
using Sequence = Primer.Timeline.Sequence;
public class SequenceReferenceSequence : Sequence
{
    public Sequence mainSequence;
    public int startClipIndex;
    public override async IAsyncEnumerator<Tween> Define()
    {
        mainSequence.Cleanup();
        
        // Declare enumerator in a using statement to ensure it is disposed
        // And therefore other using statements can be used in the sequence
        await using var enumerator = mainSequence.Define();
        
        // Apply a certain number of clips
        for (var i = 0; i < startClipIndex; i++)
        {
            await enumerator.MoveNextAsync();
            enumerator.Current.Apply();
        }

        // Yield on the rest
        while (await enumerator.MoveNextAsync())
        {
            yield return enumerator.Current;
        }
    }
}
