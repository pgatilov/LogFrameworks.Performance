using System;

namespace NLog.Performance.PerformanceMonitor
{
    internal abstract class PerformanceMetric
    {
        private readonly string _name;

        protected PerformanceMetric(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public sealed override string ToString()
        {
            return string.Format("{0}: {1}", _name, GetValueDisplayString());
        }

        public static PerformanceMetric<T> Create<T>(string name, T value)
            where T : IComparable<T>
        {
            return new PerformanceMetric<T>(name, value);
        }

        protected abstract string GetValueDisplayString();
    }

    internal class PerformanceMetric<TValue> : PerformanceMetric
        where TValue : IComparable<TValue>
    {
        private readonly TValue _value;

        public PerformanceMetric(string metricName, TValue value)
            : base(metricName)
        {
            _value = value;
        }

        public TValue Value
        {
            get { return _value; }
        }

        protected override string GetValueDisplayString()
        {
            return _value.ToString();
        }
    }
}