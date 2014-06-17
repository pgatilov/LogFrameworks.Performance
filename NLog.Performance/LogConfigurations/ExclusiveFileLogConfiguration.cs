using System.IO;

namespace NLog.Performance
{
    class ExclusiveFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "Exclusive File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddFileTarget("log.txt", exclusive: true);
        }
    }
}