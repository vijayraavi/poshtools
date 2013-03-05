using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Project;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using PowerGUIVSX;
using PowerGUIVsx.Project;
using log4net;

namespace AdamDriscoll.PowerGUIVSX
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(MyToolWindow))]
    [ProvideProjectFactory(typeof(PowerShellProjectFactory), "PowerShell", "PowerShell Project Files (*.pssproj);*.pssproj", "pssproj", "pssproj", @"\ProjectTemplates\PowerShell", LanguageVsTemplate = "PowerShell", NewProjectRequireNewFolderVsTemplate = false)]
    [ProvideProjectItem(typeof(PowerShellProjectFactory), "PowerShell", @"Templates", 500)]
    [Guid(GuidList.guidPowerGUIVSXPkgString)]
    [ProvideObject(typeof(PowerShellProjectPropertyPage))]
    [ProvideObject(typeof(PowerShellModulePropertyPage))]
    [ProvideDebugEngine("{43ACAB74-8226-4920-B489-BFCF05372437}", "PowerShell", PortSupplier = "{708C1ECA-FF48-11D2-904F-00C04FA302A1}", ProgramProvider = "{08F3B557-C153-4F6C-8745-227439E55E79}", Attach = true, CLSID = "{C7F9F131-53AB-4FD0-8517-E54D124EA392}")]
    [ClsidAttribute(Clsid = "{C7F9F131-53AB-4FD0-8517-E54D124EA392}", Assembly = "PowerGuiVsx.Core.DebugEngine", Class = "PowerGuiVsx.Core.DebugEngine.Engine")]
    [ClsidAttribute(Clsid = "{08F3B557-C153-4F6C-8745-227439E55E79}", Assembly = "PowerGuiVsx.Core.DebugEngine", Class = "PowerGuiVsx.Core.DebugEngine.ScriptProgramProvider")]
    [ProvideIncompatibleEngineInfo("{92EF0900-2251-11D2-B72E-0000F87572EF}")]
    //[ProvideIncompatibleEngineInfo("{449EC4CC-30D2-4032-9256-EE18EB41B62B}")]
    //[ProvideIncompatibleEngineInfo("{449EC4CC-30D2-4032-9256-EE18EB41B62B}")]
    [ProvideIncompatibleEngineInfo("{F200A7E7-DEA5-11D0-B854-00A0244A1DE2}")]
    public sealed class PowerGUIVSXPackage : ProjectPackage
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public PowerGUIVSXPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(MyToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidPowerGUIVSXCmdSet, (int)PkgCmdIDList.cmdidPowerGUIVSXConsole);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }

            RegisterProjectFactory(new PowerShellProjectFactory(this));

            Host = new VSXHost();
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(PowerGUIVSXPackage));

        public override string ProductUserContext
        {
            get { return "PowerShellProj"; }
        }

        [Export]
        internal VSXHost Host { get; private set; }

        public void RegisterEngine()
        {
            Log.Debug("Entering RegisterEngine().");
            RegistryKey key = null;
            try
            {
                key = ApplicationRegistryRoot.OpenSubKey(@"CLSID\{C7F9F131-53AB-4FD0-8517-E54D124EA392}");

                if (key == null)
                {
                    Log.Error("The debug engine was not installed correctly.");

                    return;
                }

                Log.DebugFormat("PowerGUI VSX Registration Key [{0}].", key.ToString());

                var assemblyLocation = GetType().Assembly.Location;

                if (String.IsNullOrEmpty(assemblyLocation) || !File.Exists(assemblyLocation))
                {
                    Log.Error("Could not properly locate the PowerGUI VSX assemblies.");
                    throw new ApplicationException("Failed to identitfy the current assembly's file path.");
                }

                var location = new FileInfo(assemblyLocation).Directory;
                Log.DebugFormat("PowerGUI VSX file location [{0}].", location);

                if (location == null)
                {
                    throw new ApplicationException("Failed to identitfy the current assembly's file path.");
                }

                var currentPath = Path.Combine(location.FullName, "PowerGuiVsx.Core.DebugEngine.dll");

                if (!Registered(key, currentPath))
                {
                    Log.Info("Debug engine is not registered. Registering debug engine.");
                    string stringKey;
                    using (key)
                    {
                        stringKey = key.ToString();
                    }

                    if (stringKey.ToUpper().StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        stringKey = stringKey.Remove(0, "HKEY_LOCAL_MACHINE\\".Length);
                        key = Registry.LocalMachine.OpenSubKey(stringKey, true);
                    }
                    else if (stringKey.ToUpper().StartsWith("HKEY_CURRENT_USER"))
                    {
                        stringKey = stringKey.Remove(0, "HKEY_CURRENT_USER\\".Length);
                        key = Registry.CurrentUser.OpenSubKey(stringKey, true);
                    }

                    if (key == null)
                    {
                        Log.Error("Debug engine was not registered correctly.");
                        return;
                    }

                    Log.DebugFormat("Setting Codebase value to [{0}].", currentPath);
                    key.SetValue("Codebase", currentPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to update assembly configuration file.", ex);
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                    key.Dispose();
                }
            }
        }

        private bool Registered(RegistryKey key, string destFile)
        {
            var value = key.GetValue("CodeBase");

            return value == null ? false : value.Equals(destFile);
        }
        #endregion

    }
}
