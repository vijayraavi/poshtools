using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ComponentModelHost;

namespace PowerShellTools.Options
{
    internal class RawHostPage : DialogPage
    {
        private int _bufferWidth;
        private int _bufferHeight;

        private readonly VisualStudioEvents _events;

        /// <summary>
        /// The constructor
        /// </summary>
        public RawHostPage()
        {
            InitializeSettings();
            var cm = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            _events = cm.GetService<VisualStudioEvents>();
        }

        [DisplayName(@"REPL Buffer Width")]
        [Description("")]
        public int BufferWidth
        {
            get
            {
                return _bufferWidth;
            }
            set
            {
                _bufferWidth = value;
                _events.OnSettingsChanged(this);
            }
        }

        [DisplayName(@"REPL Buffer Height")]
        [Description("")]
        public int BufferHeight
        {
            get 
            {
                return _bufferHeight;
            }
            set
            {
                _bufferHeight = value;
                _events.OnSettingsChanged(this);
            }
        }


        /// <summary>
        /// Initi
        /// </summary>
        private void InitializeSettings()
        {
            this._bufferWidth = 20;
            this._bufferHeight = 20;
        }
    }
}
