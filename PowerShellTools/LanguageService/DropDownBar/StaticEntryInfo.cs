using System.Management.Automation.Language;

namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Class used for tracking static elements in the navigation bar drop down.
    /// </summary>
    internal class StaticEntryInfo : IDropDownEntryInfo
    {
        private string _displayText;
        private int _imageListIndex, _start, _end;

        public StaticEntryInfo(string displayText, int imageListIndex, Ast script)
        {
            _displayText = displayText;
            _imageListIndex = imageListIndex;
            _start = script.Extent.StartOffset;
            _end = script.Extent.EndOffset;
        }
        /// <summary>
        /// Gets the text to be displayed
        /// </summary>
        public string DisplayText
        {
            get
            {
                return _displayText;
            }
        }

        /// <summary>
        /// Gets the index in our image list which should be used for the icon to be displayed
        /// </summary>
        public int ImageListIndex
        {
            get
            {
                return _imageListIndex;
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element begins
        /// </summary>
        public int Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets the position in the text buffer where the element ends
        /// </summary>
        public int End
        {
            get
            {
                return _end;
            }
        }
    }
}