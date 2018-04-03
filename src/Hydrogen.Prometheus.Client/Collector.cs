using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// A collector for a set of metrics.
    /// </summary>
    /// <remarks>
    /// Normal users should use <see cref="Gauge"/>, <see cref="Counter"/>, and <see cref="Histogram"/>.
    ///
    /// Subclasssing Collector is for advanced uses, such as proxying metrics from another monitoring system.
    /// It is it the responsibility of subclasses to ensure they produce valid metrics.
    ///
    /// See <a href="http://prometheus.io/docs/instrumenting/exposition_formats/">Exposition formats</a>
    /// </remarks>
    public abstract class Collector
    {
        private static readonly Regex MetricNameRegex = new Regex("^[a-zA-Z_:][a-zA-Z0-9_:]*$", RegexOptions.Compiled);
        private static readonly Regex MetricLabelNameRegex = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
        private static readonly Regex ReservedMetricLabelNameRegex = new Regex("^__.*", RegexOptions.Compiled);

        private protected readonly string _fullname;
        private protected readonly string _help;
        private protected readonly string[] _labelNames;

        /// <summary>
        /// The full name of the metric in the format "namespace_subsystem_name".
        /// </summary>
        public string Name => _fullname;

        /// <summary>
        /// The metric description or help text.
        /// </summary>
        public string Help => _help;

        private protected Collector(Builder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(builder.Name))
            {
                throw new ArgumentNullException(nameof(builder.Name), "Name hasn't been set.");
            }
            var name = builder.Name;
            if (!string.IsNullOrWhiteSpace(builder.Subsystem))
            {
                name = builder.Subsystem + "_" + name;
            }
            if (!string.IsNullOrWhiteSpace(builder.Namespace))
            {
                name = builder.Namespace + "_" + name;
            }
            _fullname = name;
            CheckMetricName(_fullname);

            if (string.IsNullOrWhiteSpace(builder.Help))
            {
                throw new ArgumentNullException(nameof(builder.Help), "Help hasn't been set.");
            }
            _help = builder.Help;

            _labelNames = builder.LabelNames ?? Array.Empty<string>();
            foreach (var labelName in _labelNames)
            {
                CheckMetricLabelName(labelName);
            }
        }

        /// <summary>
        /// Return all of the metrics of this Collector.
        /// </summary>
        public abstract List<MetricFamilySamples> Collect();

        /// <summary>
        /// Throws an exception if the metric name is invalid.
        /// </summary>
        /// <param name="name">The name to check.</param>
        protected static void CheckMetricName(string name)
        {
            if (!MetricNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric name: " + name);
            }
        }

        /// <summary>
        /// Throws an exception if the metric label name is invalid.
        /// </summary>
        /// <param name="name">The name to check.</param>
        protected static void CheckMetricLabelName(string name)
        {
            if (!MetricLabelNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric label name: " + name);
            }
            if (ReservedMetricLabelNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric label name, reserved for internal use: " + name);
            }
        }

        /// <summary>
        /// A collector builder.
        /// </summary>
        public abstract class Builder
        {
            /// <summary>
            /// The required name of collector. Used as the "name" field when formatting as "namespace_subsystem_name".
            /// </summary>
            public string Name { get; protected set; } = string.Empty;

            /// <summary>
            /// The optional namespace name of collector. Used as the "namespace" field when formatting as "namespace_subsystem_name".
            /// </summary>
            public string Namespace { get; protected set; } = string.Empty;

            /// <summary>
            /// The optional subsystem name of collector. Used as the "subsystem" field when formatting as "namespace_subsystem_name".
            /// </summary>
            public string Subsystem { get; protected set; } = string.Empty;

            /// <summary>
            /// The required help text for the collector.
            /// </summary>
            public string Help { get; protected set; } = string.Empty;

            /// <summary>
            /// A list of optional label names for the collector.
            /// </summary>
            public string[] LabelNames { get; protected set; } = Array.Empty<string>();
        }
    }

    /// <summary>
    /// Subclass of <see cref="Collector"/> for a specific collector type that maintains children collectors based on unique labels.
    /// </summary>
    /// <typeparam name="TChild">The concrete collector type.</typeparam>
    public abstract class Collector<TChild> : Collector
    {
        private protected readonly ConcurrentDictionary<string[], TChild> _children =
            new ConcurrentDictionary<string[], TChild>(LabelArrayEqualityComparer.Default);

        private protected TChild _noLabelsChild;

        private protected Collector(Builder builder) : base(builder) { }

        /// <summary>
        /// Return the Child with the given labels, creating it if needed.
        /// </summary>
        /// <remarks>
        /// Must be passed the same number of labels are were passed to <see cref="Collector.Builder.LabelNames"/>.
        /// </remarks>
        public TChild Labels(params string[] labelValues)
        {
            if (labelValues == null)
            {
                throw new ArgumentNullException(nameof(labelValues));
            }
            if (labelValues.Length != _labelNames.Length)
            {
                throw new ArgumentException(nameof(labelValues), "Incorrect number of labels.");
            }
            return _children.GetOrAdd(labelValues, _ => NewChild());
        }

        /// <summary>
        /// Remove the Child with the given labels.
        /// </summary>
        public void Remove(params string[] labels)
        {
            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }
            _children.TryRemove(labels, out _);
            InitializeNoLabelsChild();
        }

        /// <summary>
        /// Remove all children.
        /// </summary>
        public void Clear()
        {
            _children.Clear();
            InitializeNoLabelsChild();
        }

        private protected abstract TChild NewChild();

        private protected List<MetricFamilySamples> FamilySamplesList(CollectorType type, List<MetricFamilySamples.Sample> samples) =>
            new List<MetricFamilySamples>(1)
            {
               new MetricFamilySamples(_fullname, type, _help, samples),
            };

        private protected void InitializeNoLabelsChild()
        {
            // Initialize metric if it has no labels.
            if (_labelNames.Length == 0)
            {
                _noLabelsChild = Labels();
            }
        }

        /// <summary>
        /// A subclass of <see cref="Collector.Builder"/> which can build a specific type of collector.
        /// </summary>
        /// <typeparam name="TCollector">The specific collector type.</typeparam>
        public abstract class Builder<TCollector> : Builder
            where TCollector : Collector<TChild>
        {
            private protected abstract TCollector Create();

            private protected Builder() : base() { }

            /// <summary>
            /// Set the name of the metric. Required.
            /// </summary>
            public Builder<TCollector> WithName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Name is required.", nameof(name));
                }

                Name = name.Trim();
                return this;
            }

            /// <summary>
            /// Sets an optional namespace name.
            /// </summary>
            public Builder<TCollector> WithNamespace(string ns)
            {
                Namespace = (ns ?? string.Empty).Trim();
                return this;
            }

            /// <summary>
            /// Sets an optional subsystem name.
            /// </summary>
            public Builder<TCollector> WithSubsystem(string subsystem)
            {
                Subsystem = (subsystem ?? string.Empty).Trim();
                return this;
            }

            /// <summary>
            /// Set the help string of the metric. Required.
            /// </summary>
            public Builder<TCollector> WithHelp(string help)
            {
                if (string.IsNullOrWhiteSpace(help))
                {
                    throw new ArgumentException("Help text is required.", nameof(help));
                }

                Help = help.Trim();
                return this;
            }

            /// <summary>
            /// Set the labelNames of the metric. Optional, defaults to no labels.
            /// </summary>
            public Builder<TCollector> WithLabels(params string[] labelNames)
            {
                LabelNames = labelNames ?? throw new ArgumentNullException(nameof(labelNames));
                return this;
            }

            /// <summary>
            /// Builds the collector without registering.
            /// </summary>
            public TCollector Build() => Create();

            /// <summary>
            /// Register the Collector with the default registry.
            /// </summary>
            public TCollector Register() => Register(CollectorRegistry.DefaultRegistry);

            /// <summary>
            /// Register the Collector with the given registry.
            /// </summary>
            public TCollector Register(CollectorRegistry registry)
            {
                var collector = Create();
                registry.Register(collector);
                return collector;
            }
        }
    }
}
