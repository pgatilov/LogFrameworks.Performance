using System.IO;

namespace NLog.Performance
{
    class AsyncBufferredExclusiveFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "Async Bufferred Exclusive File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddAsyncBufferredFileTarget("log.txt", 1000, exclusive: true);
        }
    }
}