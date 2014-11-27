namespace PowerShellTools.TestAdapter
{
    /// <summary>
    /// Test results
    /// </summary>
    public enum TestResultsEnum
    {  
        Success,
        Failure,
        Inconclusive,
        Ignored,
        Skipped,
        Invalid,
        Error,
    }

    class PowerShellTestResult
    {
        public PowerShellTestResult(bool passed)
        {
            Passed = passed;
        }

        public PowerShellTestResult(bool passed, string errorMessage, string errorStacktrace)
        {
            Passed = passed;
            ErrorMessage = errorMessage;
            ErrorStacktrace = errorStacktrace;
        }

        public bool Passed { get; private set; }
        public string ErrorMessage { get; private set; }
        public string ErrorStacktrace { get; private set; }
    }
}
