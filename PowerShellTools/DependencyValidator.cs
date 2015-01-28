using System;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace PowerShellTools
{
    internal class DependencyValidator
    {
        private static readonly Version RequiredPowerShellVersion = new Version(6, 0);

        private readonly Package _package;

        public DependencyValidator(Package package)
        {
            _package = package;
        }

        private bool? _previousResult;
        public bool Validate()
        {
            if (_previousResult.HasValue) return _previousResult.Value;
            return ValidateInstalledPowerShellVersion();
        }

        public bool ValidateInstalledPowerShellVersion()
        {
            if (InstalledPowerShellVersion < RequiredPowerShellVersion)
            {
                if (!VsShellUtilities.IsInAutomationFunction(_package) && MessageBox.Show(
                    Resources.MissingPowerShellVersion,
                    Resources.MissingDependency,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://go.microsoft.com/fwlink/?LinkID=524571");
                }

                _previousResult = false;
            }
            else
            {
                _previousResult = true;    
            }

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
