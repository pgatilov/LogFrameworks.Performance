using System.Diagnostics;

namespace NLog.Performance.PerformanceMonitor
{
    internal class TotalTimeElapsedCollector : PerformanceCollectorBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public override void Start()
        {
            _stopwatch.Restart();
        }

        public override void Stop()
        {
            _stopwatch.Stop();
        }

        public override PerformanceStatistics GetPerformanceStatistics()
        {
            return new PerformanceStatistics(PerformanceMetric.Create("Total Time Elapsed", _stopwatch.Elapsed));
        }
    }
}