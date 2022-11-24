using UnityEditor;
using UnityEditor.SettingsManagement;

namespace Primer.Latex.Editor
{
    [InitializeOnLoad]
    public static class LatexSettings
    {
        const SettingsScope SCOPE = SettingsScope.User;
        const string PACKAGE = "org.primerlearning.primertools";
        const string LATEX_BIN = "general.latexBinariesDirectory";
        const string XELATEX_KEY = "general.xelatexExecutablePath";
        const string DVISVGM_KEY = "general.dvisvgmExecutablePath";
        static readonly Settings settings;


        [UserSetting("General Settings", "LaTeX Binaries Directory")]
        static readonly UserSetting<string> binDir;

        [UserSetting("General Settings", "Xelatex Executable Path")]
        static readonly UserSetting<string> xelatexPath;

        [UserSetting("General Settings", "Dvisvgm Executable Path")]
        static readonly UserSetting<string> dvisvgmPath;


        // static constructor with [InitializeOnLoad]
        // this will run when unity starts
        static LatexSettings() {
            settings = new Settings(PACKAGE);

            binDir = new UserSetting<string>(settings, LATEX_BIN, "", SCOPE);
            xelatexPath = new UserSetting<string>(settings, XELATEX_KEY, "", SCOPE);
            dvisvgmPath = new UserSetting<string>(settings, DVISVGM_KEY, "", SCOPE);

            settings.afterSettingsSaved += ReadSettings;

            ReadSettings();
        }


        [SettingsProvider]
        static SettingsProvider CreateLatexSettingsProvider() {
            return new UserSettingsProvider(
                "Preferences/LaTeX Renderer",
                settings,
                new[] { typeof(LatexSettings).Assembly }
            );
        }

        static void ReadSettings() {
            LatexBinaries.latexBinDir = binDir.value;
            LatexBinaries.xelatex = xelatexPath.value;
            LatexBinaries.dvisvgm = dvisvgmPath.value;
        }
    }
}
