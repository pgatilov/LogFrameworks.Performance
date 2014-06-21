using System.IO;

namespace NLog.Performance
{
    class MultifileAsyncBufferredExclusiveFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "2-file Async Bufferred Exclusive File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddAsyncBufferredFileGroupTarget(new[] { "log.txt", "log2.txt", "log3.txt" }, 1000, exclusive: true);
        }
    }
}