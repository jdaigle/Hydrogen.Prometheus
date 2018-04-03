using System;
using System.Collections.Generic;
using System.Threading;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// Gauge metric, to report instantaneous values.
    /// </summary>
    /// <remarks>
    /// Gauges can go both up and down.
    /// </remarks>
    public class Gauge : Collector<Gauge.Child>
    {
        /// <summary>
        /// Constructs a new Gauge collector.
        /// </summary>
        /// <param name="builder">The Gauge builder.</param>
        public Gauge(GaugeBuilder builder) : base(builder) { }

        /// <summary>
        /// Return all of the metrics of this Collector.
        /// </summary>
        public override List<MetricFamilySamples> Collect()
        {
            var samples = new List<MetricFamilySamples.Sample>(_children.Count);
            foreach (var keyValuePair in _children)
            {
                samples.Add(new MetricFamilySamples.Sample(Name, LabelNames, keyValuePair.Key, keyValuePair.Value.Value));
            }
            return FamilySamplesList(CollectorType.Gauge, samples);
        }

        private protected override Child NewChild() => new Child();

        /// <summary>
        /// Return a Builder to allow configuration of a new Gauge. Ensures required fields are provided.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="help">The help string of the metric.</param>
        public static GaugeBuilder Build(string name, string help) => (GaugeBuilder)new GaugeBuilder().WithName(name).WithHelp(help);

        /// <summary>
        /// Return a Builder to allow configuration of a new Gauge.
        /// </summary>
        public static GaugeBuilder Build() => new GaugeBuilder();

        /// <summary>
        /// Represents a unique instance of a <see cref="Hydrogen.Prometheus.Client.Gauge"/>.
        /// </summary>
        public class Child
        {
            private double _value = 0;

            internal Child() { }

            /// <summary>
            /// The current instantaneous value of the Gauge.
            /// </summary>
            public double Value => Volatile.Read(ref _value);

            /// <summary>
            /// Increment the Gauge by 1.
            /// </summary>
            public void Increment() => Increment(1);

            /// <summary>
            /// Increment the Gauge by the given amount.
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
            /// Decrement the Gauge by 1.
            /// </summary>
            public void Decrement() => Increment(-1);

            /// <summary>
            /// Decrement the Gauge by the given amount.
            /// </summary>
            public void Decrement(double value) => Increment(-value);

            /// <summary>
            /// Sets the Gauge to the given value.
            /// </summary>
            public void Set(double value) => Interlocked.Exchange(ref _value, value);

            /// <summary>
            /// Set the Gauge to the current unixtime in seconds.
            /// </summary>
            public void SetToCurrentTime() => Set(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        /// <summary>
        /// The builder for a <see cref="Gauge"/>.
        /// </summary>
        public class GaugeBuilder : Builder<Gauge>
        {
            private protected override Gauge Create() => new Gauge(this);
        }
    }
}
