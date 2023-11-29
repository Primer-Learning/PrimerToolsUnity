namespace Primer.Simulation
{
    public enum SimRecordingSettings
    {
        None, // None!
        Record, // Results will be saved to a JSON file
        VisualPlayback, // Useful for ensuring the results in a rendered run
        DataPlayback // Useful for working on scenes that depend on data, but without all the sim objects
    }
}