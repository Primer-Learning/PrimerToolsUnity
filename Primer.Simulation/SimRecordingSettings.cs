namespace Primer.Simulation
{
    public enum SimRecordingSettings
    {
        None, // None!
        Record, // Results will be saved to a JSON file
        PlaybackDataWithVisuals, // Useful for ensuring the results in a rendered run
        PlaybackDataWithoutVisuals // Useful for working on scenes that depend on data, but without all the sim objects
    }
}