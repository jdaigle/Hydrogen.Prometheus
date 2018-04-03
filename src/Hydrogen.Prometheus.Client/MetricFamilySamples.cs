using System;
using System.Collections.Generic;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// An exportable metric and all of its samples.
    /// </summary>
    public class MetricFamilySamples
    {
        /// <summary>
        /// Constructs a new <see cref="MetricFamilySamples"/>.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="type">The metric collector type.</param>
        /// <param name="help">The help text for the metric.</param>
        /// <param name="samples">A list of all samples for this metric.</param>
        public MetricFamilySamples(string name, CollectorType type, string help, List<Sample> samples)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("message", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentException("message", nameof(help));
            }

            Name = name;
            Type = type;
            Help = help;
            Samples = samples ?? throw new ArgumentNullException(nameof(samples));
        }

        /// <summary>
        /// The name of the metric.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The metric collector type.
        /// </summary>
        public CollectorType Type { get; }

        /// <summary>
        /// The help text for the metric.
        /// </summary>
        public string Help { get; }

        /// <summary>
        /// A list of all samples for this metric.
        /// </summary>
        public List<Sample> Samples { get; }

        /// <summary>
        /// A single Sample, with a unique name and set of labels.
        /// </summary>
        public class Sample
        {
            /// <summary>
            /// Constructs a single Sample.
            /// </summary>
            /// <param name="name">The name of the metric.</param>
            /// <param name="labelNames">A list of the label names for this sample.</param>
            /// <param name="labelValues">A list of the label values for this sample.</param>
            /// <param name="value">The value of this sample.</param>
            public Sample(string name, IList<string> labelNames, IList<string> labelValues, double value)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("message", nameof(name));
                }

                Name = name;
                LabelNames = labelNames ?? throw new ArgumentNullException(nameof(labelNames));
                LabelValues = labelValues ?? throw new ArgumentNullException(nameof(labelValues));
                Value = value;
            }

            /// <summary>
            /// The name of the metric.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// A list of the label names for this sample, corresponding to the <see cref="LabelValues"/>.
            /// </summary>
            public IList<string> LabelNames { get; }

            /// <summary>
            /// A list of the label values for this sample, corresponding to the <see cref="LabelNames"/>.
            /// </summary>
            public IList<string> LabelValues { get; }

            /// <summary>
            /// The value of this sample.
            /// </summary>
            public double Value { get; }
        }
    }
}
