using System.IO;

namespace NLog.Performance
{
    class KeepOpenLocalWriteAllowedFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "File Log (Keeps Open, Allows Local Writes)"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddFileTarget("log.txt", keepOpen: true, allowLocalWrite: true);
        }
    }
}