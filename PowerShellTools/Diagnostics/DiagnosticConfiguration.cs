using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace PowerShellTools.Diagnostics
{
    class DiagnosticConfiguration
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DiagnosticConfiguration));
        private static bool _initialized = false;

        public static void DisableDiagnostics()
        {
            Log.Info("Diagnostics disabled.");
            SetLoggingLevel("OFF");
        }

        private static void EnsureDiagnosticsInitialized()
        {
            if (!_initialized)
            {
                PatternLayout outputPanePattern = new PatternLayout("%date{ABSOLUTE} - %thread - %logger - %level - %message%newline");

                var appender = new OutputPaneAppender();
                appender.Layout = outputPanePattern;
                appender.ActivateOptions();

                BasicConfigurator.Configure(appender);

#if DEBUG
                PatternLayout debugOutputPattern = new PatternLayout("%date{ABSOLUTE} - %thread - %level - %message%newline");   //The DebugAppender already includes the logger.

                DebugAppender debugAppender = new DebugAppender();
                debugAppender.Layout = debugOutputPattern;
                debugAppender.ActivateOptions();

                BasicConfigurator.Configure(debugAppender);
#endif

                SetLoggingLevel("ALL");

                Log.Info("Initializing Diagnostics.");
            }
        }

        public static void EnableDiagnostics()
        {
            EnsureDiagnosticsInitialized();

            SetLoggingLevel("ALL");

            Log.Info("Diagnostics enabled.");
        }

        private static void SetLoggingLevel(string level)
        {
            log4net.Repository.ILoggerRepository[] repositories = log4net.LogManager.GetAllRepositories();

            //Configure all loggers to be at the debug level.
            foreach (log4net.Repository.ILoggerRepository repository in repositories)
            {
                repository.Threshold = repository.LevelMap[level];
                log4net.Repository.Hierarchy.Hierarchy hier = (log4net.Repository.Hierarchy.Hierarchy)repository;
                log4net.Core.ILogger[] loggers = hier.GetCurrentLoggers();
                foreach (log4net.Core.ILogger logger in loggers)
                {
                    ((log4net.Repository.Hierarchy.Logger)logger).Level = hier.LevelMap[level];
                }
            }

            //Configure the root logger.
            log4net.Repository.Hierarchy.Hierarchy h = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
            log4net.Repository.Hierarchy.Logger rootLogger = h.Root;
            rootLogger.Level = h.LevelMap[level];
        }
    }
}
