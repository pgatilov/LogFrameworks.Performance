using System;

namespace NLog.Performance.PerformanceMonitor
{
    internal interface IPerformanceCollector : IDisposable
    {
        void Start();

        void Stop();

        PerformanceStatistics GetPerformanceStatistics();
    }
}