using System.IO;

namespace NLog.Performance
{
    class SimpleFileLogConfiguration : ILogConfiguration 
    {
        public string Name
        {
            get { return "Simple File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddFileTarget("log.txt", keepOpen: false, allowLocalWrite: true);
        }
    }
}