using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Samples.CustomProject;
using PowerGUIVsx.Project;
using PowerShellTools.Project.Utils;

namespace PowerShellTools.Project
{
    [ComVisible(true)]
    [Guid("487B0655-CCEF-4E4F-BAC9-BCED12745F8D")]
    public class PowerShellModulePropertyPage : SettingsPage
    {
        private string[] _aliasesToExport;
        private string _author;
        private string _clrVersion;
        private string[] _cmdletsToExport;
        private string _companyName;
        private string _copyright;
        private string _description;
        private string _dotNetFrameworkVersion;
        private string[] _formatsToProcess;
        private string[] _functionsToExport;
        private Guid _guid;
        private string[] _moduleList;
        private string _moduleToProcess;
        private string _version;
        private string[] _nestedModules;
        private string _powershellHostName;
        private string _powershellHostVersion;
        private string _powershellVersion;
        private ProcessorArchitecture _processorArchitecture;
        private string[] _requiredAssemblies;
        private string[] _requiredModules;
        private string[] _scriptsToProcess;
        private string[] _typesToProcess;
        private string[] _variablesToExport;

        private string _manifestFileName;

        public PowerShellModulePropertyPage()
        {
            Name = "Module Settings"; //TODO:Resources.ModuleSettings;


        }

        /// <summary>
        /// Returns class FullName property value.
        /// </summary>
        public override string GetClassName()
        {
            return this.GetType().FullName;
        }

        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("ManifestFileName_DisplayName")]
        [ResourcesDescription("ManifestFileName_Description")]
      //  [TypeConverter(typeof(FileListTypeConverter))]
        public string ManifestFileName
        {
            get
            {
                return _manifestFileName;
            }
            set
            {
                _manifestFileName = value;
                IsDirty = true;
            }
        }


        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("AliasesToExport_DisplayName")]
        [ResourcesDescription("AliasesToExport_Description")]
        public string[] AliasesToExport
        {
            get
            {
                return _aliasesToExport;
            }
            set
            {
                _aliasesToExport = value;
                IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("Author_DisplayName")]
        [ResourcesDescription("Author_Description")]
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
                IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("CLRVersion_DisplayName")]
        [ResourcesDescription("ClrVersion_Description")]
        public string ClrVersion
        {
            get
            {
                return _clrVersion;   
            }
            set
            {
                _clrVersion = value;
                IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("CmdletsToExport_DisplayName")]
        [ResourcesDescription("CmdletsToExport_Description")]
        public string[] CmdletsToExport
        {
            get
            {
                return _cmdletsToExport;
            }
            set
            {
                _cmdletsToExport = value;
                IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("CompanyName_DisplayName")]
        [ResourcesDescription("CompanyName_Description")]
        public string CompanyName
        {
            get
            {
                return _companyName;
            }
            set
            {
                _companyName = value;
                IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("Copyright_DisplayName")]
        [ResourcesDescription("Copyright_Description")]
        public string Copyright
        {
            get { return _copyright; }
            set { _copyright = value;
            IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("Description_DisplayName")]
        [ResourcesDescription("Description_Description")]
        public string Description
        {
            get { return _description; }
            set { _description = value;
            IsDirty = true;
            }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("DotNetFrameworkVersion_DisplayName")]
        [ResourcesDescription("DotNetFrameworkVersion_Description")]
        public string DotNetFrameworkVersion
        {
            get { return _dotNetFrameworkVersion; }
            set { _dotNetFrameworkVersion = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("FormatsToProcess_DisplayName")]
        [ResourcesDescription("FormatsToProcess_Description")]
        public string[] FormatsToProcess
        {
            get { return _formatsToProcess; }
            set { _formatsToProcess = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("FunctionsToExport_DisplayName")]
        [ResourcesDescription("FunctionsToExport_Description")]
        public string[] FunctionsToProcess
        {
            get { return _functionsToExport; IsDirty = true; }
            set { _functionsToExport = value; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("Guid_DisplayName")]
        [ResourcesDescription("Guid_Description")]
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("ModuleList_DisplayName")]
        [ResourcesDescription("ModuleList_Description")]
        public string[] ModuleList
        {
            get { return _moduleList; }
            set { _moduleList = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("ModuleToProcess_DisplayName")]
        [ResourcesDescription("ModuleToProcess_Description")]
        public string ModuleToProcess
        {
            get { return _moduleToProcess; }
            set { _moduleToProcess = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("Version_DisplayName")]
        [ResourcesDescription("Version_Description")]
        [TypeConverter(typeof(VersionConverter))]
        public string Version
        {
            get { return _version; }
            set { _version = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("NestedModules_DisplayName")]
        [ResourcesDescription("NestedModules_Description")]
        public string[] NestedModules
        {
            get { return _nestedModules; }
            set { _nestedModules = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("PowerShellHostName_DisplayName")]
        [ResourcesDescription("PowerShellHostName_Description")]
        public string PowershellHostName
        {
            get { return _powershellHostName; }
            set { _powershellHostName = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("PowerShellHostVersion_DisplayName")]
        [ResourcesDescription("PowerShellHostVersion_Description")]
        [TypeConverter(typeof(VersionConverter))]
        public string PowershellHostVersion
        {
            get { return _powershellHostVersion; }
            set { _powershellHostVersion = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("PowerShellVersion_DisplayName")]
        [ResourcesDescription("PowerShellVersion_Description")]
        [TypeConverter(typeof(VersionConverter))]
        public string PowershellVersion
        {
            get { return _powershellVersion; }
            set { _powershellVersion = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("ProcessorArchitecture_DisplayName")]
        [ResourcesDescription("ProcessorArchitecture_Description")]
        public ProcessorArchitecture ProcessorArchitecture
        {
            get { return _processorArchitecture; }
            set { _processorArchitecture = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("RequiredAssemblies_DisplayName")]
        [ResourcesDescription("RequiredAssemblies_Description")]
        public string[] RequiredAssemblies
        {
            get { return _requiredAssemblies; }
            set { _requiredAssemblies = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("RequiredModules_DisplayName")]
        [ResourcesDescription("RequiredModules_Description")]
        public string[] RequiredModules
        {
            get { return _requiredModules; }
            set { _requiredModules = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("ScriptsToProcess_DisplayName")]
        [ResourcesDescription("ScriptsToProcess_Description")]
        public string[] ScriptsToProcess
        {
            get { return _scriptsToProcess; }
            set { _scriptsToProcess = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("TypesToProcess_DisplayName")]
        [ResourcesDescription("TypesToProcess_Description")]
        public string[] TypesToProcess
        {
            get { return _typesToProcess; }
            set { _typesToProcess = value; IsDirty = true; }
        }
        [ResourcesCategory("GeneralCategory")]
        [LocDisplayName("VariablesToExport_DisplayName")]
        [ResourcesDescription("VariablesToExport_Description")]
        public string[] VariablesToExport
        {
            get { return _variablesToExport; }
            set { _variablesToExport = value; IsDirty = true; }
        }

        private IEnumerable<PowerShellFileNode> GetFiles(HierarchyNode node)
        {
            List<PowerShellFileNode> files = new List<PowerShellFileNode>();
            var child = node.FirstChild;

            while (child != null)
            {
                if (child is PowerShellFileNode)
                {
                    files.Add(child as PowerShellFileNode);
                }
                var childsChildren = GetFiles(child);

                files.AddRange(childsChildren);

                child = child.NextSibling;
            }

            return files;
        }

        protected override void BindProperties()
        {
             if (ProjectMgr == null)
            {
                return;
            }

            
            

            _manifestFileName = this.ProjectMgr.GetProjectProperty("ManifestFileName");

            _aliasesToExport = ToStringArray(this.ProjectMgr.GetProjectProperty("AliasesToExport", true));
            _author = this.ProjectMgr.GetProjectProperty("Author", true);
            _clrVersion = this.ProjectMgr.GetProjectProperty("ClrVersion", true);
            _cmdletsToExport = ToStringArray(this.ProjectMgr.GetProjectProperty("CmdletsToExport", true));
            _companyName = this.ProjectMgr.GetProjectProperty("CompanyName", true);
            _copyright = this.ProjectMgr.GetProjectProperty("Copyright", true);
            _description = this.ProjectMgr.GetProjectProperty("Description", true);
            _dotNetFrameworkVersion = this.ProjectMgr.GetProjectProperty("DotNetFrameworkVersion", true);
            _formatsToProcess = ToStringArray(this.ProjectMgr.GetProjectProperty("FormatsToProcess", true));
            _functionsToExport = ToStringArray(this.ProjectMgr.GetProjectProperty("FunctionsToProcess", true));

            var guid = this.ProjectMgr.GetProjectProperty("Guid", true);
            if (!String.IsNullOrEmpty(guid))
            {
                _guid = new Guid(guid);
            }
           
            _moduleList =  ToStringArray(this.ProjectMgr.GetProjectProperty("ModuleList", true));
            _moduleToProcess = this.ProjectMgr.GetProjectProperty("ModuleToProcess", true);
            _version = this.ProjectMgr.GetProjectProperty("Version", true);
            _nestedModules = ToStringArray(this.ProjectMgr.GetProjectProperty("NestedModules", true));
            _powershellHostName = this.ProjectMgr.GetProjectProperty("PowerShellHostName", true);
            _powershellHostVersion = this.ProjectMgr.GetProjectProperty("PowerShellHostVersion", true);
            _powershellVersion = this.ProjectMgr.GetProjectProperty("PowerShellVersion", true);
            _processorArchitecture = ToEnum<ProcessorArchitecture>(this.ProjectMgr.GetProjectProperty("ProcessorArchitecture", true));
            _requiredAssemblies = ToStringArray(this.ProjectMgr.GetProjectProperty("RequiredAssemblies", true));
            _requiredModules =  ToStringArray(this.ProjectMgr.GetProjectProperty("RequiredModules", true));
            _scriptsToProcess = ToStringArray(this.ProjectMgr.GetProjectProperty("ScriptsToProcess", true));
            _typesToProcess = ToStringArray(this.ProjectMgr.GetProjectProperty("TypesToProcess", true));
            _variablesToExport = ToStringArray(this.ProjectMgr.GetProjectProperty("VarialesToExport", true));
        }

        private static T ToEnum<T>(string str) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException();
            }
            T e;
            return Enum.TryParse<T>(str, true, out e) ? e : default(T); 
        }
        private static Version ToVersion(string str)
        {
            Version outVersion;
            return System.Version.TryParse(str, out outVersion) ? outVersion : null;
        }

        private static string[] ToStringArray(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new string[] {};
            }
            return str.Split(';');
        }

        private string ToStringFromArray(IEnumerable<string> str)
        {
            if (str == null || !str.Any())
            {
                return string.Empty;
            }

            return str.Aggregate((x, y) => x + ";" + y);
        }

        

        protected override int ApplyChanges()
        {
            if (this.ProjectMgr == null)
            {
                return VSConstants.E_INVALIDARG;
            }

            this.ProjectMgr.SetProjectProperty("ManifestFileName", _manifestFileName);

            this.ProjectMgr.SetProjectProperty("AliasesToExport", ToStringFromArray(_aliasesToExport));
            this.ProjectMgr.SetProjectProperty("Author", _author);
            this.ProjectMgr.SetProjectProperty("ClrVersion", _clrVersion);
            this.ProjectMgr.SetProjectProperty("CmdletsToExport", ToStringFromArray(_cmdletsToExport));
            this.ProjectMgr.SetProjectProperty("CompanyName", _companyName);
            this.ProjectMgr.SetProjectProperty("Copyright", _copyright);
            this.ProjectMgr.SetProjectProperty("Description", _description);
            this.ProjectMgr.SetProjectProperty("DotNetFrameworkVersion", _dotNetFrameworkVersion);
            this.ProjectMgr.SetProjectProperty("FormatsToProcess", ToStringFromArray(_formatsToProcess));
            this.ProjectMgr.SetProjectProperty("FunctionsToProcess", ToStringFromArray(_functionsToExport));
            this.ProjectMgr.SetProjectProperty("Guid", _guid.ToString());
            this.ProjectMgr.SetProjectProperty("ModuleList", ToStringFromArray(_moduleList));
            this.ProjectMgr.SetProjectProperty("ModuleToProcess", _moduleToProcess);
            if (_version != null)
            {
                this.ProjectMgr.SetProjectProperty("Version", _version.ToString());
            }
            
            this.ProjectMgr.SetProjectProperty("NestedModules", ToStringFromArray(_nestedModules));
            this.ProjectMgr.SetProjectProperty("PowerShellHostName", _powershellHostName);
            if (_powershellHostVersion != null)
            {
                this.ProjectMgr.SetProjectProperty("PowerShellHostVersion", _powershellHostVersion.ToString());    
            }

            if (_powershellVersion != null)
            {
                this.ProjectMgr.SetProjectProperty("PowerShellVersion", _powershellVersion.ToString());    
            }
            
            this.ProjectMgr.SetProjectProperty("ProcessorArchitecture", _processorArchitecture.ToString());
            this.ProjectMgr.SetProjectProperty("RequiredAssemblies", ToStringFromArray(_requiredAssemblies));
            this.ProjectMgr.SetProjectProperty("RequiredModules", ToStringFromArray(_requiredModules));
            this.ProjectMgr.SetProjectProperty("ScriptsToProcess", ToStringFromArray(_scriptsToProcess));
            this.ProjectMgr.SetProjectProperty("TypesToProcess", ToStringFromArray(_typesToProcess));
            this.ProjectMgr.SetProjectProperty("VarialesToExport", ToStringFromArray(_variablesToExport));

            IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}
