namespace NLog.Performance.PerformanceMonitor
{
    internal abstract class PerformanceCollectorBase : IPerformanceCollector
    {
        public virtual void Dispose()
        {
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract PerformanceStatistics GetPerformanceStatistics();
    }
}