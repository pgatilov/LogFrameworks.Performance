using System;
using System.Threading;

namespace NLog.Performance
{
    class LargeMessageScenario : IScenario 
    {
        private static readonly string Message;

        static LargeMessageScenario()
        {
            Message = new string('a', 100 * 1024);
        }

        public string Name { get { return "Large Message (100 Kb) + 100 ms db query"; } }

        public void Run(ILogImplementation logger)
        {
            logger.WriteMessage(Message);

            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }
}