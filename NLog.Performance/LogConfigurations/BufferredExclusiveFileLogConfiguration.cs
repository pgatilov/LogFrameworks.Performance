using System.IO;

namespace NLog.Performance
{
    class BufferredExclusiveFileLogConfiguration : ILogConfiguration
    {
        public string Name
        {
            get { return "Bufferred Exclusive File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            if (File.Exists("log.txt"))
            {
                File.Delete("log.txt");
            }

            implementation.ClearTargets();
            implementation.AddBufferredFileTarget("log.txt", 1000, exclusive: true);
        }
    }
}