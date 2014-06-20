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

        private static FileTarget CreateFileTarget(string fileName, bool keepOpen = false, bool allowLocalWrite = true) 
        {
            var fileTarget = new FileTarget
                {
                    FileName = Layout.FromString(fileName),
                    Layout = Layout.FromString("${message}")
                };

            if (keepOpen)
            {
                fileTarget.KeepFileOpen = true;

                fileTarget.NetworkWrites = false;
                fileTarget.ConcurrentWrites = allowLocalWrite;
            }

            return fileTarget;
        }

        public void ClearTargets() 
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ReconfigExistingLoggers();
        }

        public void AddFileTarget(string fileName, bool keepOpen, bool allowLocalWrite)
        {
            var fileTarget = CreateFileTarget(fileName, keepOpen, allowLocalWrite);
            SetupLoggingWithTarget(fileTarget);
        }

        public void AddBufferredFileTarget(string fileName, int bufferSize, bool exclusive)
        {
            var fileTarget = CreateFileTarget(fileName, keepOpen: exclusive, allowLocalWrite: !exclusive);
            var bufferredTarget = new BufferingTargetWrapper(fileTarget, bufferSize, 50);
            SetupLoggingWithTarget(bufferredTarget);
        }

        public void AddAsyncBufferredFileTarget(string fileName, int bufferSize, bool exclusive)
        {
            var fileTarget = CreateFileTarget(fileName, keepOpen: exclusive, allowLocalWrite: !exclusive);
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
            _logger.Info(message, (object[])null);
            // NLog bug! Do not do it this way for strings! It will pass it into string.Format("{0}", yourStringHere) which is a waste of resources (-15% log write performance!)
            //_logger.Info(message);
        }
    }
}