using System;
using System.Collections.Generic;
using System.Management.Automation.Language;
using System.Security;
using EnvDTE80;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using PowerShellTools.Classification;
using PowerShellTools.Commands.UserInterface;

namespace PowerShellTools.Commands
{
    /// <summary>
    /// Command for executing a script with parameters prompt from the editor context menu.
    /// </summary>
    internal sealed class ExecuteWithParametersAsScriptCommand : ExecuteFromEditorContextMenuCommand
    {
        private string _scriptArgs;
        private IVsTextManager _textManager;
        private IVsEditorAdaptersFactoryService _adaptersFactory;
        private Ast _paramBlock;

        internal ExecuteWithParametersAsScriptCommand(IVsEditorAdaptersFactoryService adaptersFactory, IVsTextManager textManager, IDependencyValidator validator)
            : base(validator)
        {
            _adaptersFactory = adaptersFactory;
            _textManager = textManager;
        }

        protected override string ScriptArgs
        {
            get
            {
                if (_scriptArgs == null)
                {
                    _scriptArgs = GetScriptParamters();
                }
                return _scriptArgs;
            }
        }

        protected override int Id
        {
            get { return (int)GuidList.CmdidExecuteWithParametersAsScript; }
        }

        protected override bool ShouldShowCommand(DTE2 dte2)
        {
            return dte2 != null &&
                   dte2.ActiveDocument != null &&
                   dte2.ActiveDocument.Language == "PowerShell" &&
                   HasParameters();
        }

        private bool HasParameters()
        {
            // TODO: Parse script to see if there are paramters
            IVsTextView vsTextView;
            _textManager.GetActiveView(1, null, out vsTextView);
            if (vsTextView == null)
            {
                return false;                
            }

            IVsTextLines textLines;
            vsTextView.GetBuffer(out textLines);
            ITextBuffer textBuffer = _adaptersFactory.GetDataBuffer(textLines as IVsTextBuffer);
            Ast scriptAst;
            if (!textBuffer.Properties.TryGetProperty<Ast>(BufferProperties.Ast, out scriptAst))
            {
                return false;
            }
            _paramBlock = scriptAst.Find(p => p is ParamBlockAst, false);

            return _paramBlock != null;
        }

        private string GetScriptParamters()
        {
            string scriptArgs;
            if (ShowParameterEditor(out scriptArgs) == true)
            {
                return scriptArgs;
            }
            return String.Empty;
        }

        private bool? ShowParameterEditor(out string scriptArgs)
        {
            var parameters = ReadParametersFromScript();
            var viewModel = new ParameterEditorViewModel(parameters);
            var view = new ParameterEditorView(viewModel);
            bool? wasOkClicked = view.ShowModal();
            scriptArgs = parameters[0].Value.ToString();
            return wasOkClicked;
        }

        private IList<ScriptParameterViewModel> ReadParametersFromScript()
        {
            return new ScriptParameterViewModel[]
            {
#if DEBUG
             
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkEmpty", Type="string" })
                        { 
                            Value="",
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkNull", Type="string" })
                        { 
                            Value=null,
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="StringWithWatermarkNonNull", Type="string" })
                        { 
                            Value="hi"
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="SecureStringWithWatermark", Type="string" })
                        { 
                            Value=new SecureString()
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BoolWithWatermark", Type="bool" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="IntWithWatermark", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="GoodString", Type="string" })
                        { 
                            Value="string value #1" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadString1", Type="string" })
                        { 
                            Value=3
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadString2", Type="string" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullString", Type="string" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyString", Type="string" })
                        { 
                            Value="" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="GoodInt", Type="int" })
                        { 
                            Value=314
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadInt1", Type="int" })
                        { 
                            Value="bad int"
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadInt2", Type="int" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullInt", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyInt", Type="int" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="GoodPassword", Type="securestring" })
                        { 
                            Value="My Password" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadPassword1", Type="securestring" })
                        { 
                            Value=1234
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadPassword2", Type="securestring" })
                        { 
                            Value=true
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullPassword", Type="securestring" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyPassword", Type="securestring" })
                        { 
                            Value=""
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="TrueBoolean", Type="bool" })
                        { 
                            Value=true
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="FalseBoolean", Type="bool" })
                        { 
                            Value=false
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadBoolean1", Type="bool" })
                        { 
                            Value="bad bool" 
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="BadBoolean2", Type="bool" })
                        { 
                            Value=314
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="NullBoolean", Type="bool" })
                        { 
                            Value=null
                        },
                        new ScriptParameterViewModel(new ScriptParameter() { Name="EmptyBoolean", Type="bool" })
                        { 
                            Value=null
                        },
                        //new ScriptParameterViewModel(new ScriptParameter() { Name="BadType", Type="badtype" })
                        //{ 
                        //    Value=null
                        //},
                    
#endif
            };
        }
    }
}
