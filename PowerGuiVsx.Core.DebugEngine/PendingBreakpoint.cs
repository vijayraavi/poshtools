namespace PowerShellTools.DebugEngine
{
    public enum BreakpointType
    {
        Line,
        Command
    }

    public class PendingBreakpoint
    {
        public string Context { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public BreakpointType BreakpointType { get; set; }
        public string Language { get; set; }
    }
}
