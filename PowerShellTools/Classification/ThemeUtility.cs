using System;
using System.Collections.Generic;
using System.Windows.Media;
using log4net;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;

namespace PowerShellTools.Classification
{
    enum VsTheme
    {
        Unknown = 0,
        Light,
        Dark,
        Blue
    }

    class ThemeUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (ThemeUtil));
        public const string TextEditorFontCategoryGuidString = "A27B4E24-A735-4D1D-B8E7-9716E1E3D8E0";
        static Guid _textEditorFontCategoryGuid = new Guid(TextEditorFontCategoryGuidString);

        private static readonly IDictionary<string, VsTheme> Themes = new Dictionary<string, VsTheme>
        {
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VsTheme.Light }, 
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VsTheme.Dark }, 
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VsTheme.Blue }
        };

        public static VsTheme GetCurrentTheme()
        {
            string themeId = GetThemeId();
            if (string.IsNullOrWhiteSpace(themeId) == false)
            {
                VsTheme theme;
                if (Themes.TryGetValue(themeId, out theme))
                {
                    return theme;
                }
            }

            return VsTheme.Unknown;
        }

        public static string GetThemeId()
        {
            const string categoryName = "General";
            const string themePropertyName = "CurrentTheme";
            //TODO: This needs to switch based on the Version of Visual Studio
            string keyName = string.Format(@"Software\Microsoft\VisualStudio\11.0\{0}", categoryName);

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    return (string)key.GetValue(themePropertyName, string.Empty);
                }
            }

            return null;
        }

        public static IDefaultColors GetDefaultColors()
        {
            if (GetCurrentTheme() == VsTheme.Dark)
            {
                return new DarkThemeDefaultColors();
            }
            return new LightThemeDefaultColors();
        }

        public static void UpdateColorsForTheme(IVsFontAndColorStorage fontAndColorStorage)
        {
            Log.Debug("Updating colors for the current theme.");
            var defaultColors = GetDefaultColors();
            const uint flags = (uint)(
              __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS);

            if (fontAndColorStorage.OpenCategory(ref _textEditorFontCategoryGuid, flags) != VSConstants.S_OK)
            {
                Log.Error("Failed to open the text editor category.");
                return;
            }

            //Reload the default colors. It'll pick up the correctly changed theme's colors. 
            SetColor(fontAndColorStorage, "PowerShell Attribute");
            SetColor(fontAndColorStorage, "PowerShell Command");
            SetColor(fontAndColorStorage, "PowerShell Command Parameter");
            SetColor(fontAndColorStorage, "PowerShell Command Argument");
            SetColor(fontAndColorStorage, "PowerShell Comment");
            SetColor(fontAndColorStorage, "PowerShell Group End");
            SetColor(fontAndColorStorage, "PowerShell Group Start");
            SetColor(fontAndColorStorage, "PowerShell Keyword");
            SetColor(fontAndColorStorage, "PowerShell Member");
            SetColor(fontAndColorStorage, "PowerShell Number");
            SetColor(fontAndColorStorage, "PowerShell Operator");
            SetColor(fontAndColorStorage, "PowerShell String");
            SetColor(fontAndColorStorage, "PowerShell Type");
            SetColor(fontAndColorStorage, "PowerShell Variable");

            fontAndColorStorage.CloseCategory();
        }

        private static void SetColor(IVsFontAndColorStorage fontsAndColorStorage, string displayName)
        {
            var cii = new ColorableItemInfo[1];
            fontsAndColorStorage.GetItem(displayName, cii);
            fontsAndColorStorage.SetItem(displayName, cii);
        }

        private static ColorableItemInfo[] ToColorableItemInfo(Color defaultColor)
        {
            var color = System.Drawing.Color.FromArgb(defaultColor.A, defaultColor.R, defaultColor.G, defaultColor.B);

            return new[] { new ColorableItemInfo { crForeground = ((uint)__VSCOLORTYPE.CT_RAW | (uint)color.ToArgb()), bForegroundValid = 1 } };
        }
    }
}

