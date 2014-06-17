using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Performance.PerformanceMonitor
{
    internal class PerformanceStatistics
    {
        private readonly IReadOnlyCollection<PerformanceMetric> _reportItems;

        public PerformanceStatistics(params PerformanceMetric[] metrics)
            : this((IEnumerable<PerformanceMetric>)metrics)
        {
        }

        public PerformanceStatistics(IEnumerable<PerformanceMetric> reportItems)
        {
            _reportItems = reportItems.ToList().AsReadOnly();
        }

        public IReadOnlyCollection<PerformanceMetric> ReportItems
        {
            get { return _reportItems; }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, _reportItems);
        }
    }
}