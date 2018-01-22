using System;
using System.Collections.Generic;
using System.Linq;

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

        //public override bool Equals(object obj)
        //{
        //    var other = obj as MetricFamilySamples;
        //    if (other == null)
        //    {
        //        return false;
        //    }
        //    return other.Name == Name
        //        && other.Type == Type
        //        && other.Help == Help
        //        && other.Samples.SequenceEqual(Samples);
        //}

        //public override int GetHashCode()
        //{
        //    int hashCode = 1;
        //    hashCode = 37 * hashCode + Name.GetHashCode();
        //    hashCode = 37 * hashCode + Type.GetHashCode();
        //    hashCode = 37 * hashCode + Help.GetHashCode();
        //    hashCode = 37 * hashCode + Samples.GetHashCode();
        //    return hashCode;
        //}

        public class Sample
        {
            public Sample(string name, string[] labelNames, string[] labelValues, double value)
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

            public string[] LabelNames { get; }

            public string[] LabelValues { get; }

            public double Value { get; }
        }
    }
}
