using System.IO;

namespace NLog.Performance
{
    class AsyncBufferredFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "Async Bufferred File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddAsyncBufferredFileTarget("log.txt", 1000, exclusive: false);
        }
    }
}