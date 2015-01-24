using System;
using System.Windows;
using Microsoft.Win32;

namespace PowerShellTools
{
    public class DependencyValidator
    {
        private bool? _previousResult;
        public bool Validate()
        {
            if (_previousResult.HasValue) return _previousResult.Value;
            return ValidateInstalledPowerShellVersion();
        }

        public bool ValidateInstalledPowerShellVersion()
        {
            if (InstalledPowerShellVersion < new Version(3, 0))
            {
                if (MessageBox.Show(
                    Resources.MissingPowerShellVersion,
                    Resources.MissingDependency,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://go.microsoft.com/fwlink/?LinkID=524571");
                }

                _previousResult = false;
                return _previousResult.Value;
            }

            _previousResult = true;
            return _previousResult.Value;
        }

        public static Version InstalledPowerShellVersion
        {
            get
            {
                var version = new Version(0, 0);
                using (var reg = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell\3\PowerShellEngine"))
                {
                    if (reg != null)
                    {
                        var versionString = reg.GetValue("PowerShellVersion") as string;

                        Version.TryParse(versionString, out version);
                        return version;
                    }
                }

                using (var reg = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\PowerShell\1\PowerShellEngine"))
                {
                    if (reg != null)
                    {
                        var versionString = reg.GetValue("PowerShellVersion") as string;
                        Version.TryParse(versionString, out version);
                        return version;
                    }
                }

                return version;
            }
        }
    }
}
