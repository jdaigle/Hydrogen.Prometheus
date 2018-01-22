using System;
using System.Collections.Generic;
using System.Linq;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    public class Histogram : Collector<Histogram.Child>
    {
        private readonly double[] _buckets;

        public Histogram(HistogramBuilder builder)
            : base(builder)
        {
            _buckets = builder.Buckets;
        }

        public override List<MetricFamilySamples> Collect()
        {
            var samples = new List<MetricFamilySamples.Sample>(_children.Count);
            foreach (var keyValuePair in _children)
            {
                var labelNamesWithLe = new List<string>(_labelNames)
                {
                    "le"
                };
                var (buckets, sum) = keyValuePair.Value.GetValue();
                for (int i = 0; i < buckets.Length; ++i)
                {
                    var labelValuesWithLe = new List<string>(keyValuePair.Key)
                    {
                        StringHelpers.DoubleToGoString(buckets[i])
                    };
                    samples.Add(new MetricFamilySamples.Sample(_fullname + "_bucket", labelNamesWithLe, labelValuesWithLe, buckets[i]));
                }

                samples.Add(new MetricFamilySamples.Sample(_fullname + "_count", _labelNames, keyValuePair.Key, buckets[buckets.Length - 1]));
                samples.Add(new MetricFamilySamples.Sample(_fullname + "_sum", _labelNames, keyValuePair.Key, sum));
            }
            return FamilySamplesList(CollectorType.Histogram, samples);
        }

        protected override Child NewChild() => throw new NotImplementedException();

        /// <summary>
        /// Return a Builder to allow configuration of a new Historgram. Ensures required fields are provided.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="help">The help string of the metric.</param>
        public static HistogramBuilder Build(string name, string help) => (HistogramBuilder)new HistogramBuilder().WithName(name).WithHelp(help);

        /// <summary>
        /// Return a Builder to allow configuration of a new Historgram.
        /// </summary>
        public static HistogramBuilder Build() => new HistogramBuilder();

        public class Child
        {
            private readonly double[] _upperBounds;
            private readonly double[] _cumulativeCounts;
            private double _sum;

            public Child(double[] buckets)
            {
                _upperBounds = buckets;
                _cumulativeCounts = new double[buckets.Length];
            }

            /// <summary>
            /// Observe the given amount.
            /// </summary>
            /// <param name="value">The amount to observe.</param>
            public void Observe(double value)
            {
                for (int i = 0; i < _upperBounds.Length; ++i)
                {
                    // The last bucket is +Inf, so we always increment.
                    if (value <= _upperBounds[i])
                    {
                        ThreadSafeDouble.Add(ref _upperBounds[i], value);
                        break;
                    }
                }
                ThreadSafeDouble.Add(ref _sum, value);
            }

            public (double[] buckets, double sum) GetValue()
            {
                var buckets = new double[_cumulativeCounts.Length];
                double accum = 0;
                for (var i = 0; i < _cumulativeCounts.Length; i++)
                {
                    accum += _cumulativeCounts[i];
                    buckets[i] = accum;
                }
                return (buckets, _sum);
            }
        }

        public class HistogramBuilder : Builder<Histogram>
        {
            private static readonly double[] _defaultBuckets =
                new double[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 };

            public HistogramBuilder WithBuckets(params double[] buckets)
            {
                Buckets = buckets ?? _defaultBuckets;
                return this;
            }

            /// <summary>
            /// Set the upper bounds of buckets for the histogram with a linear sequence.
            /// </summary>
            /// <param name="start">The first upper bound.</param>
            /// <param name="width">The width of each bucket.</param>
            /// <param name="count">The number of buckets.</param>
            public HistogramBuilder LinearBuckets(double start, double width, int count)
            {
                Buckets = new double[count];
                for (var i = 0; i < count; i++)
                {
                    Buckets[i] = start + i * width;
                }
                return this;
            }

            /// <summary>
            /// Set the upper bounds of buckets for the histogram with an exponential sequence.
            /// </summary>
            /// <param name="start">The first upper bound.</param>
            /// <param name="factor">The expon</param>
            /// <param name="count"></param>
            /// <returns></returns>
            public HistogramBuilder ExponentialBuckets(double start, double factor, int count)
            {
                Buckets = new double[count];
                for (var i = 0; i < count; i++)
                {
                    Buckets[i] = start * Math.Pow(factor, i);
                }
                return this;
            }

            protected override Histogram Create()
            {
                if (Buckets.Length == 0)
                {
                    throw new InvalidOperationException("Histogram must have at least one bucket.");
                }

                for (var i = 0; i < Buckets.Length - 1; i++)
                {
                    if (Buckets[i] >= Buckets[i + 1])
                    {
                        throw new InvalidOperationException("Histogram buckets must be in increasing order: "
                            + Buckets[i] + " >= " + Buckets[i + 1]);
                    }
                }

                // Append infinity bucket if it's not already there.
                if (Buckets[Buckets.Length - 1] != double.PositiveInfinity)
                {
                    var tmp = new double[Buckets.Length + 1];
                    Array.Copy(Buckets, tmp, Buckets.Length);
                    tmp[Buckets.Length] = double.PositiveInfinity;
                    Buckets = tmp;
                }

                if (LabelNames.Any(x => x.Equals("le")))
                {
                    throw new InvalidOperationException("Histogram cannot have a label named 'le'.");
                }

                return new Histogram(this);
            }

            public double[] Buckets { get; private set; } = _defaultBuckets;
        }
    }
}
