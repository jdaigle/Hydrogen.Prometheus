using System;
using System.Collections.Generic;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    public class Counter : Collector<Counter.Child>
    {
        public Counter(CounterBuilder builder)
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
            return FamilySamplesList(CollectorType.Counter, samples);
        }

        protected override Child NewChild() => new Child();

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

        public class Child
        {
            private double _value = 0;

            internal Child() { }

            public double Value => _value;

            /// <summary>
            /// Increment the counter by 1.
            /// </summary>
            public void Increment() => Increment(1);

            /// <summary>
            /// Increment the counter by the given amount.
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

        public class CounterBuilder : Builder<Counter>
        {
            protected override Counter Create() => new Counter(this);
        }
    }
}
