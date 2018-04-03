using System;
using System.Collections.Generic;
using System.Linq;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// Histogram metric, to track distributions of events.
    /// </summary>
    /// <remarks>
    /// Note: Each bucket is one timeseries. Many buckets and/or many dimensions with labels
    /// can produce large amount of time series, that may cause performance problems.
    ///
    /// The default buckets are intended to cover a typical web/rpc request from milliseconds to seconds.
    ///
    /// <see cref="HistogramBuilder.LinearBuckets(double, double, int)"/> and
    /// <see cref="HistogramBuilder.ExponentialBuckets(double, double, int)"/>
    /// offer easy ways to set common bucket patterns.
    /// </remarks>
    public class Histogram : Collector<Histogram.Child>
    {
        private readonly double[] _buckets;

        /// <summary>
        /// Constructs a new Histogram collector.
        /// </summary>
        /// <param name="builder">The Histogram builder.</param>
        public Histogram(HistogramBuilder builder)
            : base(builder)
        {
            _buckets = builder.Buckets;
        }

        /// <summary>
        /// Return all of the metrics of this Collector.
        /// </summary>
        public override List<MetricFamilySamples> Collect()
        {
            var samples = new List<MetricFamilySamples.Sample>(_children.Count);
            foreach (var keyValuePair in _children)
            {
                var labelNamesWithLe = new List<string>(LabelNames)
                {
                    "le"
                };
                var (buckets, sum) = keyValuePair.Value.GetValue();
                for (int i = 0; i < buckets.Length; ++i)
                {
                    var labelValuesWithLe = new List<string>(keyValuePair.Key)
                    {
                        StringExtensions.ConvertToGoString(buckets[i])
                    };
                    samples.Add(new MetricFamilySamples.Sample(Name + "_bucket", labelNamesWithLe, labelValuesWithLe, buckets[i]));
                }

                samples.Add(new MetricFamilySamples.Sample(Name + "_count", LabelNames, keyValuePair.Key, buckets[buckets.Length - 1]));
                samples.Add(new MetricFamilySamples.Sample(Name + "_sum", LabelNames, keyValuePair.Key, sum));
            }
            return FamilySamplesList(CollectorType.Histogram, samples);
        }

        private protected override Child NewChild() => throw new NotImplementedException();

        /// <summary>
        /// Return a Builder to allow configuration of a new Histogram. Ensures required fields are provided.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="help">The help string of the metric.</param>
        public static HistogramBuilder Build(string name, string help) => (HistogramBuilder)new HistogramBuilder().WithName(name).WithHelp(help);

        /// <summary>
        /// Return a Builder to allow configuration of a new Histogram.
        /// </summary>
        public static HistogramBuilder Build() => new HistogramBuilder();

        /// <summary>
        /// Represents a unique instance of a <see cref="Hydrogen.Prometheus.Client.Histogram"/>.
        /// </summary>
        public class Child
        {
            private readonly double[] _upperBounds;
            private readonly double[] _cumulativeCounts;

            private double _sum;

            internal Child(double[] buckets)
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

            internal (double[] buckets, double sum) GetValue()
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

        /// <summary>
        /// The builder for a <see cref="Histogram"/>.
        /// </summary>
        public class HistogramBuilder : Builder<Histogram>
        {
            private static readonly double[] _defaultBuckets =
                new double[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 };

            /// <summary>
            /// The specified buckets for this Histogram.
            /// </summary>
            public double[] Buckets { get; private set; } = _defaultBuckets;

            /// <summary>
            /// Set the upper bounds of buckets for the Histogram.
            /// </summary>
            /// <param name="buckets">The Histogram buckets</param>
            public HistogramBuilder WithBuckets(params double[] buckets)
            {
                Buckets = buckets ?? _defaultBuckets;
                return this;
            }

            /// <summary>
            /// Set the upper bounds of buckets for the Histogram with a linear sequence.
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
            /// Set the upper bounds of buckets for the Histogram with an exponential sequence.
            /// </summary>
            /// <param name="start">The first upper bound.</param>
            /// <param name="factor">The expon</param>
            /// <param name="count"></param>
            public HistogramBuilder ExponentialBuckets(double start, double factor, int count)
            {
                Buckets = new double[count];
                for (var i = 0; i < count; i++)
                {
                    Buckets[i] = start * Math.Pow(factor, i);
                }
                return this;
            }

            private protected override Histogram Create()
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
        }
    }
}
