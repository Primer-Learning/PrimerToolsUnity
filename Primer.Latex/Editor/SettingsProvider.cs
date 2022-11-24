using UnityEditor.SettingsManagement;

namespace UnityEditor.LatexRenderer.UserSettings
{
    internal static class Provider
    {
        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            // The last parameter tells the provider where to search for settings.
            var provider = new UserSettingsProvider("Preferences/Latex Renderer", Manager.Instance,
                new[] { typeof(Provider).Assembly });

            return provider;
        }
    }
}