namespace PrimerGraph
{
    public enum ArrowPresence
    {
        Neither,
        Positive,
        Both
    }

    // public static class ArrowPresenceExtensions
    // {
    //     [Obsolete("Use PrimerGraph.ArrowPresence enum instead")]
    //     public static ArrowPresence ToArrowPresence(this string arrowPresence) {
    //         switch (arrowPresence) {
    //             case "neither": return ArrowPresence.Neither;
    //             case "positive": return ArrowPresence.Positive;
    //             case "both": return ArrowPresence.Both;
    //         }
    //         throw new InvalidCastException($"{arrowPresence} is not a valid arrow presence");
    //     }
    // }
}
