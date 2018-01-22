using System;
using System.Collections.Generic;

namespace Hydrogen.Prometheus.Client
{
    public class MetricFamilySamples
    {
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

        public string Name { get; }

        public CollectorType Type { get; }

        public string Help { get; }

        public List<Sample> Samples { get; }

        public class Sample
        {
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

            public string Name { get; }

            public IList<string> LabelNames { get; }

            public IList<string> LabelValues { get; }

            public double Value { get; }
        }
    }
}
