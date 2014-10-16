using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using XmlTestAdapter;
using XmlTestAdapter.EventWatchers;
using XmlTestAdapter.EventWatchers.EventArgs;

namespace PowerShellTools.TestAdapter.Pester
{
    [Export(typeof(ITestContainerDiscoverer))]
    public class PesterTestContainerDiscoverer : ITestContainerDiscoverer
    {
        public const string ExecutorUriString = "executor://PesterTestExecutor/v1";
   
        public event EventHandler TestContainersUpdated;
        private IServiceProvider serviceProvider;
        private ILogger logger;
        private ISolutionEventsListener solutionListener;
        private ITestFilesUpdateWatcher testFilesUpdateWatcher;
        private ITestFileAddRemoveListener testFilesAddRemoveListener;
        private bool initialContainerSearch;
        private readonly List<ITestContainer> cachedContainers;
        protected string FileExtension { get { return ".ps1"; } }
        public Uri ExecutorUri { get { return new System.Uri(ExecutorUriString); } }
        public IEnumerable<ITestContainer> TestContainers   {get { return GetTestContainers(); }   }

        [ImportingConstructor]
        public PesterTestContainerDiscoverer(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            ILogger logger,
            ISolutionEventsListener solutionListener,
            ITestFilesUpdateWatcher testFilesUpdateWatcher,
            ITestFileAddRemoveListener testFilesAddRemoveListener)
        {
            initialContainerSearch = true;
            cachedContainers = new List<ITestContainer>();
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.solutionListener = solutionListener;
            this.testFilesUpdateWatcher = testFilesUpdateWatcher;
            this.testFilesAddRemoveListener = testFilesAddRemoveListener;

            logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer Constructor Entering");

            this.testFilesAddRemoveListener.TestFileChanged += OnProjectItemChanged;
            this.testFilesAddRemoveListener.StartListeningForTestFileChanges();

            this.solutionListener.SolutionUnloaded += SolutionListenerOnSolutionUnloaded;
            this.solutionListener.SolutionProjectChanged += OnSolutionProjectChanged;
            this.solutionListener.StartListeningForChanges();

            this.testFilesUpdateWatcher.FileChangedEvent += OnProjectItemChanged;
        }

        private void OnTestContainersChanged()
        {
            logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnTestContainersChanged");
            if (TestContainersUpdated != null && !initialContainerSearch)
            {
                logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:Triggering on TestContainersUpdated");
                TestContainersUpdated(this, EventArgs.Empty);
            }
        }

        private void SolutionListenerOnSolutionUnloaded(object sender, EventArgs eventArgs)
        {
            initialContainerSearch = true;
        }

        private void OnSolutionProjectChanged(object sender, SolutionEventsListenerEventArgs e)
        {
            logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnSolutionProjectChanged");
            if (e != null)
            {
                var files = FindPs1Files(e.Project);
                if (e.ChangedReason == SolutionChangedReason.Load)
                {
                    logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnTestContainersChanged - Change reason is load");
                    UpdateFileWatcher(files, true);
                }
                else if (e.ChangedReason == SolutionChangedReason.Unload)
                {
                    logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnTestContainersChanged - Change reason is unload");
                    UpdateFileWatcher(files, false);
                }
            }

            // Do not fire OnTestContainersChanged here.
            // This will cause us to fire this event too early before the UTE is ready to process containers and will result in an exception.
            // The UTE will query all the TestContainerDiscoverers once the solution is loaded.
        }

        private void UpdateFileWatcher(IEnumerable<string> files, bool isAdd)
        {
            foreach (var file in files)
            {
                if (isAdd)
                {
                    logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:UpdateFileWatcher - AddWatch:" + file);
                    testFilesUpdateWatcher.AddWatch(file);
                    AddTestContainerIfTestFile(file);
                }
                else
                {
                    logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:UpdateFileWatcher - RemoveWatch:" + file);
                    testFilesUpdateWatcher.RemoveWatch(file);
                    RemoveTestContainer(file);
                }
            }
        }


        private void OnProjectItemChanged(object sender, TestFileChangedEventArgs e)
        {
            logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnProjectItemChanged");
            if (e != null)
            {
                // Don't do anything for files we are sure can't be test files
                if (!IsPs1File(e.File)) return;

                logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnProjectItemChanged - IsPs1File");

                switch (e.ChangedReason)
                {
                    case TestFileChangedReason.Added:
                        logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnProjectItemChanged - Added");
                        testFilesUpdateWatcher.AddWatch(e.File);
                        AddTestContainerIfTestFile(e.File);

                        break;
                    case TestFileChangedReason.Removed:
                        logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnProjectItemChanged - Removed");
                        testFilesUpdateWatcher.RemoveWatch(e.File);
                        RemoveTestContainer(e.File);

                        break;
                    case TestFileChangedReason.Changed:
                        logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnProjectItemChanged - Changed");
                        AddTestContainerIfTestFile(e.File);
                        break;
                }

                OnTestContainersChanged();
            }
        }

        private void AddTestContainerIfTestFile(string file)
        {
            var isTestFile = IsTestFile(file);
            RemoveTestContainer(file); // Remove if there is an existing container

            // If this is a test file
            if (isTestFile)
            {
                logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:AddTestContainerIfTestFile - Is a test file. Adding to cached containers.");
                var container = new PesterTestContainer(this, file, ExecutorUri);
                cachedContainers.Add(container);
            }
        }

        private void RemoveTestContainer(string file)
        {
            var index = cachedContainers.FindIndex(x => x.Source.Equals(file, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                logger.Log(MessageLevel.Diagnostic, String.Format("PesterTestContainerDiscoverer:RemoveTestContainer - Removing [{0}] from cached containers.", file));
                cachedContainers.RemoveAt(index);
            }
        }

        private IEnumerable<ITestContainer> GetTestContainers()
        {
            if (initialContainerSearch)
            {
                cachedContainers.Clear();
                var xmlFiles = FindPs1Files();
                UpdateFileWatcher(xmlFiles, true);
                initialContainerSearch = false;
            }

            return cachedContainers;
        }

        private IEnumerable<string> FindPs1Files()
        {
            var solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            var loadedProjects = solution.EnumerateLoadedProjects(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION).OfType<IVsProject>();

            return loadedProjects.SelectMany(FindPs1Files).ToList();
        }

        private IEnumerable<string> FindPs1Files(IVsProject project)
        {
            logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:OnTestContainersChanged - FindPs1Files");
            return from item in VsSolutionHelper.GetProjectItems(project)
                   where IsTestFile(item)
                   select item;
        }

        private static bool IsPs1File(string path)
        {
            return ".ps1".Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsTestFile(string path)
        {
            try
            {
                logger.Log(MessageLevel.Diagnostic, "PesterTestContainerDiscoverer:IsTestFile - " + path);
                return IsPs1File(path);
            }
            catch (IOException e)
            {
                logger.Log(MessageLevel.Error, "IO error when detecting a test file during Test Container Discovery" + e.Message);
            }

            return false;
        }


        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (testFilesUpdateWatcher != null)
                {
                    testFilesUpdateWatcher.FileChangedEvent -= OnProjectItemChanged;
                    ((IDisposable)testFilesUpdateWatcher).Dispose();
                    testFilesUpdateWatcher = null;
                }

                if (testFilesAddRemoveListener != null)
                {
                    testFilesAddRemoveListener.TestFileChanged -= OnProjectItemChanged;
                    testFilesAddRemoveListener.StopListeningForTestFileChanges();
                    testFilesAddRemoveListener = null;
                }

                if (solutionListener != null)
                {
                    solutionListener.SolutionProjectChanged -= OnSolutionProjectChanged;
                    solutionListener.StopListeningForChanges();
                    solutionListener = null;
                }
            }
        }


    }
}
