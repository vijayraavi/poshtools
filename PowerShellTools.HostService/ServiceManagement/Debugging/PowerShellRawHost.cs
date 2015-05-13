using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;
using PowerShellTools.HostService.ServiceManagement;
using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System.Windows.Input;
using System.Management.Automation;

namespace PowerShellTools.HostService.ServiceManagement.Debugging
{
    /// <summary>
    /// Implementation of the PSHostRawUserInterface.
    /// Members of this class that easily map to the .NET 
    /// console class are implemented. More complex methods are not 
    /// implemented and throw a NotImplementedException exception.
    /// </summary>
    internal sealed class PowerShellRawHost : PSHostRawUserInterface
    {
        private readonly PowerShellDebuggingService _debuggingService;

        public PowerShellRawHost(PowerShellDebuggingService debugger)
        {
            _debuggingService = debugger;
        }

        /// <summary>
        /// Gets or sets the foreground color of the displayed text.
        /// This maps to the corresponding Console.ForgroundColor property.
        /// </summary>
        public override ConsoleColor ForegroundColor
        {
            get
            {
                return _debuggingService.RawHostOptions.ForegroundColor;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the background color of the displayed text.
        /// This maps to the corresponding Console.Background property.
        /// </summary>
        public override ConsoleColor BackgroundColor
        {
            get
            {
                return _debuggingService.RawHostOptions.BackgroundColor;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the cursor position. In this example this 
        /// functionality is not needed so the property throws a 
        /// NotImplementException exception.
        /// </summary>
        public override Coordinates CursorPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size of the displayed cursor. In this example 
        /// the cursor size is taken directly from the Console.CursorSize 
        /// property.
        /// </summary>
        public override int CursorSize
        {
            get
            {
                return _debuggingService.RawHostOptions.CursorSize;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the size of the host buffer. In this example the 
        /// buffer size is adapted from the Console buffer size members.
        /// </summary>
        public override Size BufferSize
        {
            get
            {
                return _debuggingService.RawHostOptions.BufferSize;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the position of the displayed window. This example 
        /// uses the Console window position APIs to determine the returned 
        /// value of this property.
        /// </summary>
        public override Coordinates WindowPosition
        {
            get
            {
                return _debuggingService.RawHostOptions.WindowPosition;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the size of the displayed window. This example 
        /// uses the corresponding Console window size APIs to determine the  
        /// returned value of this property.
        /// </summary>
        public override Size WindowSize
        {
            get
            {
                return _debuggingService.RawHostOptions.WindowSize;
            }
            set { }
        }

        /// <summary>
        /// Gets the dimentions of the largest window size that can be 
        /// displayed. This example uses the Console.LargestWindowWidth and 
        /// console.LargestWindowHeight properties to determine the returned 
        /// value of this property.
        /// </summary>
        public override Size MaxWindowSize
        {
            get
            {
                return _debuggingService.RawHostOptions.MaxWindowSize;
            }
        }

        /// <summary>
        /// Gets the dimensions of the largest window that could be 
        /// rendered in the current display, if the buffer was at the least 
        /// that large. This example uses the Console.LargestWindowWidth and 
        /// Console.LargestWindowHeight properties to determine the returned 
        /// value of this property.
        /// </summary>
        public override Size MaxPhysicalWindowSize
        {
            get
            {
                return _debuggingService.RawHostOptions.MaxPhysicalWindowSize;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has pressed a key. This maps   
        /// to the corresponding Console.KeyAvailable property.
        /// </summary>
        public override bool KeyAvailable
        {
            get
            {
                return _debuggingService.CallbackService.IsKeyAvailable();
            }
        }

        /// <summary>
        /// Gets or sets the title of the displayed window. The example 
        /// maps the Console.Title property to the value of this property.
        /// </summary>
        public override string WindowTitle
        {
            get
            {
                return _debuggingService.RawHostOptions.WindowTitle;
            }
            set { }
        }

        /// <summary>
        /// This API resets the input buffer. In this example this 
        /// functionality is not needed so the method returns nothing.
        /// </summary>
        public override void FlushInputBuffer()
        {
        }

        /// <summary>
        /// This API returns a rectangular region of the screen buffer. In 
        /// this example this functionality is not needed so the method throws 
        /// a NotImplementException exception.
        /// </summary>
        /// <param name="rectangle">Defines the size of the rectangle.</param>
        /// <returns>Throws a NotImplementedException exception.</returns>
        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException(
                     "The method or operation is not implemented.");
        }

        /// <summary>
        /// This API reads a pressed, released, or pressed and released keystroke 
        /// from the keyboard device, blocking processing until a keystroke is 
        /// typed that matches the specified keystroke options. In this example 
        /// this functionality is not needed so the method throws a
        /// NotImplementException exception.
        /// </summary>
        /// <param name="options">Options, such as IncludeKeyDown,  used when reading the keyboard.</param>
        /// <returns>KeyInfo of the key pressed</returns>
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            VsKeyInfo keyInfo = _debuggingService.CallbackService.VsReadKey();

            if (keyInfo == null)
            {
                // abort current pipeline
                throw new PipelineStoppedException();
            }

            ControlKeyStates states = default(ControlKeyStates);
            states |= (keyInfo.CapsLockToggled ? ControlKeyStates.CapsLockOn : 0);
            states |= (keyInfo.NumLockToggled ? ControlKeyStates.NumLockOn : 0);
            states |= (keyInfo.ShiftPressed ? ControlKeyStates.ShiftPressed : 0);
            states |= (keyInfo.AltPressed ? ControlKeyStates.LeftAltPressed : 0); // assume LEFT alt
            states |= (keyInfo.ControlPressed ? ControlKeyStates.LeftCtrlPressed : 0); // assume LEFT ctrl

            return new KeyInfo(keyInfo.VirtualKey, keyInfo.KeyChar, states, keyDown: (keyInfo.KeyStates == KeyStates.Down));
        }

        /// <summary>
        /// This API crops a region of the screen buffer. In this example 
        /// this functionality is not needed so the method throws a
        /// NotImplementException exception.
        /// </summary>
        /// <param name="source">The region of the screen to be scrolled.</param>
        /// <param name="destination">The region of the screen to receive the 
        /// source region contents.</param>
        /// <param name="clip">The region of the screen to include in the operation.</param>
        /// <param name="fill">The character and attributes to be used to fill all cell.</param>
        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        /// <summary>
        /// This method copies an array of buffer cells into the screen buffer 
        /// at a specified location. In this example this functionality is 
        /// not needed so the method throws a NotImplementedException exception.
        /// </summary>
        /// <param name="origin">The parameter is not used.</param>
        /// <param name="contents">The parameter is not used.</param>
        public override void SetBufferContents(Coordinates origin,
                                               BufferCell[,] contents)
        {
            throw new NotImplementedException(
                      "The method or operation is not implemented.");
        }

        /// <summary>
        /// This method copies a given character, foreground color, and background 
        /// color to a region of the screen buffer. 
        /// </summary>
        /// <param name="rectangle">Defines the area to be filled. </param>
        /// <param name="fill">Defines the fill character.</param>
        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            // Reference: get-command clear-host | Select-Object -expand Definition
            if (rectangle.Top == -1 &&
                rectangle.Bottom == -1 &&
                rectangle.Left == -1 &&
                rectangle.Right == -1 &&
                fill.Character == ' ')
            {
                _debuggingService.CallbackService.ClearHostScreen();
            }
        }
    }
}
