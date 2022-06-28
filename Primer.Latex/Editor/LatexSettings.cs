using UnityEditor.SettingsManagement;

namespace UnityEditor.LatexRenderer.UserSettings
{
    internal static class LatexSettings
    {
        [UserSetting("General Settings", "Xelatex Executable Path")]
        internal static UserSetting<string> xelatexExecutablePath = new UserSetting<string>(
            Manager.Instance, "general.xelatexExecutablePath", "", SettingsScope.User);
        
        [UserSetting("General Settings", "Dvisvgm Executable Path")]
        internal static UserSetting<string> dvisvgmExecutablePath = new UserSetting<string>(
            Manager.Instance, "general.dvisvgmExecutablePath", "", SettingsScope.User);
    }
}