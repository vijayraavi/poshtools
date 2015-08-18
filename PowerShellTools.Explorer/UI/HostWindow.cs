using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace PowerShellTools.Explorer
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("dd9b7693-1385-46a9-a054-06566904f861")]
    public class HostWindow : ToolWindowPane, IHostWindow
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IDataProvider _dataProvider;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public HostWindow() :
            base(null)
        {
            _exceptionHandler = new ExceptionHandler();
            _dataProvider = new DataProvider(_exceptionHandler);

            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new PSCommandExplorer(this, _dataProvider, _exceptionHandler);
        }

        public void Close()
        {
            // For IHostWindow implementation only
        }
    }
}
