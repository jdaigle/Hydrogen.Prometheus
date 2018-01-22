using System;
using System.Collections.Generic;
using System.Threading;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    public class Gauge : Collector<Gauge.Child>
    {
        public Gauge(GaugeBuilder builder)
            : base(builder)
        {
        }

        public override List<MetricFamilySamples> Collect()
        {
            var samples = new List<MetricFamilySamples.Sample>(_children.Count);
            foreach (var keyValuePair in _children)
            {
                samples.Add(new MetricFamilySamples.Sample(_fullname, _labelNames, keyValuePair.Key, keyValuePair.Value.Value));
            }
            return FamilySamplesList(CollectorType.Guage, samples);
        }

        public class Child
        {
            private double _value = 0;

            public double Value => _value;

            /// <summary>
            /// Increment the gauge by 1.
            /// </summary>
            public void Increment() => Increment(1);

            /// <summary>
            /// Increment the gauge by the given amount.
            /// </summary>
            public void Increment(double value)
            {
                if (value == 0)
                {
                    return;
                }

                ThreadSafeDouble.Add(ref _value, value);
            }

            /// <summary>
            /// Decrement the guage by 1.
            /// </summary>
            public void Decrement() => Increment(-1);

            /// <summary>
            /// Decrement the guage by the given amount.
            /// </summary>
            public void Decrement(double value) => Increment(-value);

            /// <summary>
            /// Sets the guage to the given value.
            /// </summary>
            public void Set(double value) => Interlocked.Exchange(ref _value, value);

            /// <summary>
            /// Set the gauge to the current unixtime in seconds.
            /// </summary>
            public void SetToCurrentTime() => Set(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        public class GaugeBuilder : Builder<Gauge>
        {
            protected override Gauge Create() => new Gauge(this);
        }
    }
}
