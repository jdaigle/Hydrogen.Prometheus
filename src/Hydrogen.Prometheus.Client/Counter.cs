using System;
using System.Collections.Generic;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// Counter metric, to track counts of events or running totals.
    /// </summary>
    /// <remarks>
    /// Counters can only go up (and be reset), if your use case can go down you should use a <see cref="Gauge"/> instead.
    /// Use the <c>rate()</c> function in Prometheus to calculate the rate of increase of a Counter.
    /// By convention, the names of Counters are suffixed by <c>_total</c>
    /// </remarks>
    public class Counter : Collector<Counter.Child>
    {
        /// <summary>
        /// Constructs a new Counter collector.
        /// </summary>
        /// <param name="builder">The Counter builder.</param>
        public Counter(CounterBuilder builder) : base(builder) { }

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
            return FamilySamplesList(CollectorType.Counter, samples);
        }

        private protected override Child NewChild() => new Child();

        /// <summary>
        /// Return a Builder to allow configuration of a new Counter. Ensures required fields are provided.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="help">The help string of the metric.</param>
        public static CounterBuilder Build(string name, string help) => (CounterBuilder)new CounterBuilder().WithName(name).WithHelp(help);

        /// <summary>
        /// Return a Builder to allow configuration of a new Counter.
        /// </summary>
        public static CounterBuilder Build() => new CounterBuilder();

        /// <summary>
        /// Represents a unique instance of a <see cref="Hydrogen.Prometheus.Client.Counter"/>.
        /// </summary>
        public class Child
        {
            private double _value = 0;

            internal Child() { }

            /// <summary>
            /// The current Counter value.
            /// </summary>
            public double Value => _value;

            /// <summary>
            /// Increment the Counter by 1.
            /// </summary>
            public void Increment() => Increment(1);

            /// <summary>
            /// Increment the Counter by the given amount.
            /// </summary>
            public void Increment(double inc)
            {
                if (inc < 0)
                {
                    throw new ArgumentOutOfRangeException("Amount to increment must be non-negative.");
                }

                if (inc == 0)
                {
                    return;
                }

                ThreadSafeDouble.Add(ref _value, inc);
            }
        }

        /// <summary>
        /// The builder for a <see cref="Counter"/>.
        /// </summary>
        public class CounterBuilder : Builder<Counter>
        {
            private protected override Counter Create() => new Counter(this);
        }
    }
}
