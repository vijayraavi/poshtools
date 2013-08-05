using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Samples.CustomProject;

namespace PowerShellTools.Project
{
    [ComVisible(true)]
    [Guid("37633C86-B9C5-4391-BBF1-1E7C2960F9F8")]
    public class PowerShellProjectPropertyPage : SettingsPage
    {
        private bool _createModule;
        private bool _signoutput;
        private string _cert;
        private string _outputDirectory;
        private string _startupScript;

        public PowerShellProjectPropertyPage()
        {
            Name = "General Settings"; //TODO:Resources.GeneralSettings;
        }

        protected override void BindProperties()
        {
            if (ProjectMgr == null)
            {
                return;
            }

            var outputType = this.ProjectMgr.GetProjectProperty("CreateModule", true);

            if (!String.IsNullOrEmpty(outputType))
            {
                Boolean.TryParse(outputType, out _createModule);
            }

            var signoutput  = this.ProjectMgr.GetProjectProperty("SignOutput", true);

            if (!String.IsNullOrEmpty(signoutput))
            {
                Boolean.TryParse(signoutput, out _signoutput);
            }

            _cert = this.ProjectMgr.GetProjectProperty("CodeSigningCert", true);
            _outputDirectory = this.ProjectMgr.GetProjectProperty("OutDir", true);
            _startupScript = this.ProjectMgr.GetProjectProperty("StartupScript", true);
        }

        protected override int ApplyChanges()
        {
            if (this.ProjectMgr == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            this.ProjectMgr.SetProjectProperty("CreateModule", _createModule.ToString());
            this.ProjectMgr.SetProjectProperty("SignOutput", _signoutput.ToString());
            this.ProjectMgr.SetProjectProperty("CodeSigningCert", _cert);
            this.ProjectMgr.SetProjectProperty("OutDir", _outputDirectory);
            this.ProjectMgr.SetProjectProperty("StartupScript", _startupScript);

            IsDirty = false;

            return VSConstants.S_OK;
        }

        [ResourcesCategory("OutputCategory")]
        [LocDisplayName("SignOutputSetting")]
        [ResourcesDescription("SignOutputSettingDescription")]
        public bool SignOutput
        {
            get
            {
                return _signoutput;
            }
            set
            {
                _signoutput = value;
                IsDirty = true;
            }
        }

        [ResourcesCategory("OutputCategory")]
        [LocDisplayName("OutputDirectory_DisplayName")]
        [ResourcesDescription("OutputDirectory_Description")]
        public string OutputDirectory
        {
            get
            {
                return _outputDirectory;
            }
            set
            {
                _outputDirectory = value;
                IsDirty = true;
            }
        }

        [ResourcesCategory("OutputCategory")]
        [LocDisplayName("CodeCertSetting")]
        [ResourcesDescription("CodeCertSettingDesc")]
        public string CodeSigningCertificate
        {
            get
            {
                return _cert;
            }
            set
            {
                _cert = value;
                IsDirty = true;
            }
        }

        [ResourcesCategory("OutputCategory")]
        [LocDisplayName("OutputTypeSetting")]
        [ResourcesDescription("OutputTypeSettingDescription")]
        public bool CreateModule
        {
            get
            {
                return _createModule;
            }
            set
            {
                _createModule = value;
                IsDirty = true;
            }
        }

        [ResourcesCategory("DebugCategory")]
        [LocDisplayName("StartupScriptSetting")]
        [ResourcesDescription("StartupScriptSettingDescription")]
        public string StartupScript
        {
            get
            {
                return _startupScript;
            }
            set
            {
                _startupScript = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Returns class FullName property value.
        /// </summary>
        public override string GetClassName()
        {
            return this.GetType().FullName;
        }


    }

    public enum OutputType
    {
        Module,
        Script
    }
}
