using System;

namespace NLog.Performance.PerformanceMonitor
{
    internal interface IPerformanceMonitor : IDisposable
    {
        void Start();

        void Stop();

        PerformanceStatistics GetPerformanceStatistics();
    }
}