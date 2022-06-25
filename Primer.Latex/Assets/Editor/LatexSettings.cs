using UnityEditor.SettingsManagement;

namespace UnityEditor.LatexRenderer.UserSettings
{
    internal static class LatexSettings
    {
        [UserSetting("General Settings", "Latex Executable Path")]
        internal static UserSetting<string> latexExecutablePath = new UserSetting<string>(
            Manager.Instance, "general.latexExecutablePath", "", SettingsScope.User);
        
        [UserSetting("General Settings", "Dvisvgm Executable Path")]
        internal static UserSetting<string> dvisvgmExecutablePath = new UserSetting<string>(
            Manager.Instance, "general.dvisvgmExecutablePath", "", SettingsScope.User);
    }
}