namespace PowerShellTools.LanguageService.DropDownBar
{
    /// <summary>
    /// Class used for tracking static elements in the navigation bar drop down.
    /// </summary>
    internal class StaticEntryInfo : IDropDownEntryInfo
    {
        private string _name;
        private int _imageListIndex, _start, _end;

        public StaticEntryInfo(string name, int imageListIndex, int start, int end)
        {
            _name = name;
            _imageListIndex = imageListIndex;
            _start = start;
            _end = end;
        }
        /// <summary>
        /// Gets the name to be displayed
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
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