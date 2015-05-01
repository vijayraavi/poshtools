/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;
using PowerShellTools.Classification;

namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Implements the navigation bar which appears above a source file in the editor.
    /// 
    /// The navigation bar consists of two drop-down boxes.  On the left hand side is a list
    /// of top level constructs.  On the right hand side are list of nested constructs for the
    /// currently selected top-level construct.
    /// 
    /// When the user moves the caret the current selections are automatically updated.  If the
    /// user is inside of a top level construct but not inside any of the available nested 
    /// constructs then the first element of the nested construct list is selected and displayed
    /// grayed out.  If the user is inside of no top level constructs then the 1st top-level
    /// construct is selected and displayed as grayed out.  It's first top-level construct is
    /// also displayed as being grayed out.
    /// 
    /// The most difficult part of this is handling the transitions from one state to another.
    /// We need to change the current selections due to events from two sources:  The first is selections
    /// in the drop down and the 2nd is the user navigating within the source code.  When a change
    /// occurs we may need to update the left hand side (along w/ a corresponding update to the right
    /// hand side) or we may need to update the right hand side.  If we are transitioning from
    /// being outside of a known element to being in a known element we also need to refresh 
    /// the drop down to remove grayed out elements.
    /// </summary>
    internal class DropDownBarClient : IVsDropdownBarClient
    {
        private readonly Dispatcher _dispatcher;                        // current dispatcher so we can get back to our thread
        private readonly IWpfTextView _textView;                        // text view we're drop downs for
        private IVsDropdownBar _dropDownBar;                            // drop down bar - used to refresh when changes occur
        private ReadOnlyCollection<IDropDownEntryInfo> _topLevelEntries; // entries for top-level members of the file
        private ReadOnlyCollection<IDropDownEntryInfo> _nestedEntries;   // entries for nested members in the file
        private int _topLevelIndex = -1, _nestedIndex = -1;       // currently selected indices for each bar

        private static readonly ImageList _imageList = GetImageList();
        private IPowerShellTokenizationService _tokenizer;

        public DropDownBarClient(IWpfTextView textView)
        {
            Utilities.ArgumentNotNull("textView", textView);
            _textView = textView;

            if (_textView.TextBuffer.ContentType.IsOfType(PowerShellConstants.LanguageName))
            {
                _textView.TextBuffer.Properties.TryGetProperty(BufferProperties.PowerShellTokenizer, out _tokenizer);
            }
            else
            {
                _tokenizer = new PowerShellTokenizationService(textView.TextBuffer);
            }

            _dispatcher = Dispatcher.CurrentDispatcher;
            _textView.Caret.PositionChanged += Caret_PositionChanged;
            _tokenizer.TokenizationComplete += Tokenizer_TokenizationComplete;
        }

        internal void Unregister()
        {
            _textView.Caret.PositionChanged -= Caret_PositionChanged;
            _tokenizer.TokenizationComplete -= Tokenizer_TokenizationComplete;
        }

        #region IVsDropdownBarClient Members

        /// <summary>
        /// Gets the attributes for the specified combo box.  We return the number of elements that we will
        /// display, the various attributes that VS should query for next (text, image, and attributes of
        /// the text such as being grayed out), along with the appropriate image list.
        /// 
        /// We always return the # of entries based off our entries list, the exact same image list, and
        /// we have VS query for text, image, and text attributes all the time.
        /// </summary>
        public int GetComboAttributes(int iCombo, out uint pcEntries, out uint puEntryType, out IntPtr phImageList)
        {
            var entries = GetEntries(iCombo);
            if (entries != null)
            {
                pcEntries = (uint)entries.Count;
            }
            else
            {
                pcEntries = 0;
            }

            puEntryType = (uint)(DROPDOWNENTRYTYPE.ENTRY_TEXT | DROPDOWNENTRYTYPE.ENTRY_IMAGE | DROPDOWNENTRYTYPE.ENTRY_ATTR);
            phImageList = _imageList.Handle;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the tool tip for the given combo box.
        /// </summary>
        public int GetComboTipText(int iCombo, out string pbstrText)
        {
            pbstrText = null;

            if (iCombo == ComboBoxId.Nested)
            {
                var index = GetSelectedIndex(iCombo);
                var entries = GetEntries(iCombo);
                if (entries != null && index != -1 && index < entries.Count)
                {
                    pbstrText = entries[index].DisplayText + "\n\n" + Resources.DropDownToolTip;
                }
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the entry attributes for the given combo box and index.
        /// </summary>
        public int GetEntryAttributes(int iCombo, int iIndex, out uint pAttr)
        {
            pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_PLAIN;

            var entries = GetEntries(iCombo);
            var selectedIndex = GetSelectedIndex(iCombo);
            var caretPosition = _textView.Caret.Position.BufferPosition.Position;
            if (entries != null && iIndex < entries.Count &&
                iIndex == selectedIndex &&
                (caretPosition < entries[selectedIndex].Start ||
                 caretPosition > entries[selectedIndex].End))
            {
                pAttr = (uint)DROPDOWNFONTATTR.FONTATTR_GRAY;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the image for the given combo box and index.
        /// </summary>
        public int GetEntryImage(int iCombo, int iIndex, out int piImageIndex)
        {
            piImageIndex = 0;

            var entries = GetEntries(iCombo);
            if (entries != null && iIndex < entries.Count)
            {
                piImageIndex = entries[iIndex].ImageListIndex;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the text displayed for the given combo box and index.
        /// </summary>
        public int GetEntryText(int iCombo, int iIndex, out string ppszText)
        {
            ppszText = String.Empty;

            var entries = GetEntries(iCombo);
            if (entries != null && iIndex < entries.Count)
            {
                ppszText = entries[iIndex].DisplayText;
            }

            return VSConstants.S_OK;
        }

        public int OnComboGetFocus(int iCombo)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called when the user selects an item from the drop down.  We will
        /// update the caret to beat the correct location, move the view port
        /// so that the code is centered on the screen, and we may refresh
        /// the combo box so that the 1st item is no longer grayed out if
        /// the user was originally outside of valid selection.
        /// </summary>
        public int OnItemChosen(int iCombo, int iIndex)
        {
            if (_dropDownBar == null)
            {
                return VSConstants.E_UNEXPECTED;
            }

            var entries = GetEntries(iCombo);
            if (entries !=null && iIndex < entries.Count)
            {
                SetSelectedIndex(iCombo, iIndex);
                _dropDownBar.RefreshCombo(iCombo, iIndex);

                var functionEntryInfo = entries[iIndex] as FunctionDefinitionEntryInfo;
                if (functionEntryInfo != null)
                {
                    NavigationExtensions.NavigateToFunctionDefinition(_textView, functionEntryInfo.FunctionDefinition);
                }
                else
                {
                    NavigationExtensions.NavigateToLocation(_textView, entries[iIndex].Start);
                }
            }

            return VSConstants.S_OK;
        }

        public int OnItemSelected(int iCombo, int iIndex)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by VS to provide us with the drop down bar.  We can call back
        /// on the drop down bar to force VS to refresh the combo box or change
        /// the current selection.
        /// </summary>
        public int SetDropdownBar(IVsDropdownBar pDropdownBar)
        {
            _dropDownBar = pDropdownBar;

            ParseError[] errors;
            Token[] tokens;
            var ast = Parser.ParseInput(_textView.TextBuffer.CurrentSnapshot.GetText(), out tokens, out errors);
            UpdateDropDownEntries(ast);

            return VSConstants.S_OK;
        }

        #endregion

        #region Selection Synchronization

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // At the moment, the topLevel drop down box never changes, so we only update the nested drop down box
            SetActiveSelection(ComboBoxId.Nested);
        }

        private void Tokenizer_TokenizationComplete(object sender, Ast ast)
        {
            if (_dropDownBar != null)
            {
                Action callback = () => {
                    UpdateDropDownEntries(ast);
                };
                _dispatcher.BeginInvoke(callback, DispatcherPriority.Background);
            }
        }

        private void UpdateDropDownEntries(Ast ast)
        {
            _topLevelEntries = CalculateTopLevelEntries(ast);
            SetActiveSelection(ComboBoxId.TopLevel);
            _nestedEntries = CalculateNestedEntries(ast);
            SetActiveSelection(ComboBoxId.Nested);
        }

        private void SetActiveSelection(int comboBoxId)
        {
            if (_dropDownBar != null)
            {
                var newIndex = -1;

                var entries = GetEntries(comboBoxId);
                if (entries != null && entries.Any())
                {
                    var newPosition = _textView.Caret.Position.BufferPosition.Position;
                    var entriesByScope = entries.OrderBy(entry => entry.End);
                    var activeEntry = entriesByScope.FirstOrDefault(entry => newPosition >= entry.Start && newPosition <= entry.End);

                    if (activeEntry != null)
                    {
                        newIndex = entries.IndexOf(activeEntry);
                    }
                    else
                    {
                        // If outside all entries, select the entry just before it
                        var closestEntry = entriesByScope.LastOrDefault(entry => newPosition >= entry.End);
                        if (closestEntry == null)
                        {
                            // if the mouse is before any entries, select the first one
                            closestEntry = entries.OrderBy(entry => entry.Start).First();
                        }

                        newIndex = entries.IndexOf(closestEntry);
                    }
                }

                SetSelectedIndex(comboBoxId, newIndex);
                _dropDownBar.RefreshCombo(comboBoxId, newIndex);
            }
        }

        #endregion

        #region Entry Calculation

        /// <summary>
        /// Reads our image list from our DLLs resource stream.
        /// </summary>
        private static ImageList GetImageList()
        {
            ImageList list = new ImageList();
            list.ImageSize = new Size(0x10, 0x10);
            list.TransparentColor = Color.FromArgb(0xff, 0, 0xff);
            Stream manifestResourceStream = typeof(DropDownBarClient).Assembly.GetManifestResourceStream("PowerShellTools.Resources.completionset.bmp");
            list.Images.AddStrip(new Bitmap(manifestResourceStream));
            return list;
        }

        private static ReadOnlyCollection<IDropDownEntryInfo> CalculateTopLevelEntries(Ast script)
        {
            var newEntries = new Collection<IDropDownEntryInfo>();

            if (script != null)
            {
                newEntries.Add(new StaticEntryInfo("(Script)", (int)ImageListKind.Class, script));
            }

            return new ReadOnlyCollection<IDropDownEntryInfo>(newEntries);
        }

        private static ReadOnlyCollection<IDropDownEntryInfo> CalculateNestedEntries(Ast script)
        {
            List<IDropDownEntryInfo> newEntries = new List<IDropDownEntryInfo>();

            if (script != null)
            {
                foreach (var function in script.FindAll(node => node is FunctionDefinitionAst, true).Cast<FunctionDefinitionAst>())
                {
                    newEntries.Add(new FunctionDefinitionEntryInfo(function));
                }
            }

            newEntries.Sort((x, y) => String.CompareOrdinal(x.DisplayText, y.DisplayText));
            return new ReadOnlyCollection<IDropDownEntryInfo>(newEntries);
        }

        private static class ComboBoxId
        {
            public const int TopLevel = 0;
            public const int Nested = 1;
        }

        private ReadOnlyCollection<IDropDownEntryInfo> GetEntries(int iCombo)
        {
            switch (iCombo)
            {
                case ComboBoxId.TopLevel:
                    return _topLevelEntries;
                case ComboBoxId.Nested:
                    return _nestedEntries;
                default:
                    return null;
            }
        }

        private int GetSelectedIndex(int iCombo)
        {
            switch (iCombo)
            {
                case ComboBoxId.TopLevel:
                    return _topLevelIndex;
                case ComboBoxId.Nested:
                    return _nestedIndex;
                default:
                    return -1;
            }
        }

        private void SetSelectedIndex(int iCombo, int iIndex)
        {
            switch (iCombo)
            {
                case ComboBoxId.TopLevel:
                    _topLevelIndex = iIndex;
                    break;
                case ComboBoxId.Nested:
                    _nestedIndex = iIndex;
                    break;
            }
        }
        #endregion
    }
}
