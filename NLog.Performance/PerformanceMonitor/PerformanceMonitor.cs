using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Performance.PerformanceMonitor
{
    internal sealed class PerformanceMonitor : IPerformanceMonitor
    {
        private readonly IReadOnlyCollection<IPerformanceCollector> _collectors;

        public PerformanceMonitor()
        {
            _collectors = PerformanceCollectorsFactory.CreateCollectors().ToList().AsReadOnly();
        }

        public void Dispose()
        {
            var disposeErrors = new List<Exception>();
            foreach (var performanceCollector in _collectors)
            {
                try
                {
                    performanceCollector.Dispose();
                }
                catch (Exception e)
                {
                    disposeErrors.Add(e);
                }
            }

            if (disposeErrors.Any())
            {
                throw new AggregateException("Several errors have occurred while disposing performance collectors.", disposeErrors);
            }
        }

        public void Start()
        {
            foreach (var performanceCollector in _collectors)
            {
                performanceCollector.Start();
            }
        }

        public void Stop()
        {
            foreach (var performanceCollector in _collectors)
            {
                performanceCollector.Stop();
            }
        }

        public PerformanceStatistics GetPerformanceStatistics()
        {
            var allMetrics = from collector in _collectors
                             let report = collector.GetPerformanceStatistics()
                             from metric in report.ReportItems
                             select metric;
            return new PerformanceStatistics(allMetrics);
        }
    }
}