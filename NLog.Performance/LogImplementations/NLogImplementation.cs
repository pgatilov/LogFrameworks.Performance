using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace NLog.Performance.LogImplementations
{
    class NLogImplementation : ILogImplementation
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Name { get { return "NLog"; } }

        private static FileTarget CreateFileTarget(string fileName) 
        {
            var fileTarget = new FileTarget
                {
                    FileName = Layout.FromString(fileName),
                    Layout = Layout.FromString("${message}")
                };
            return fileTarget;
        }

        public void ClearTargets() 
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ReconfigExistingLoggers();
        }

        public void AddFileTarget(string fileName)
        {
            var fileTarget = CreateFileTarget(fileName);
            SetupLoggingWithTarget(fileTarget);
        }

        public void AddBufferredFileTarget(string fileName, int bufferSize)
        {
            var fileTarget = CreateFileTarget(fileName);
            var bufferredTarget = new BufferingTargetWrapper(fileTarget, bufferSize, 100);
            SetupLoggingWithTarget(bufferredTarget);
        }

        public void AddAsyncBufferredFileTarget(string fileName, int bufferSize)
        {
            var fileTarget = CreateFileTarget(fileName);
            var bufferredTarget = new AsyncTargetWrapper(fileTarget, bufferSize, AsyncTargetWrapperOverflowAction.Block);
            SetupLoggingWithTarget(bufferredTarget);
        }

        private void SetupLoggingWithTarget(Target target)
        {
            var configuration = LogManager.Configuration;

            configuration.AddTarget("target", target);

            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, target));

            LogManager.ReconfigExistingLoggers();
        }

        public void WriteMessage(string message) 
        {
            _logger.Info(message);
        }
    }
}