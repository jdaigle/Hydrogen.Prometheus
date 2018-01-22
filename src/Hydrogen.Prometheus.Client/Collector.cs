﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client
{
    public abstract class Collector
    {
        private static readonly Regex MetricNameRegex = new Regex("[a-zA-Z_:][a-zA-Z0-9_:]*", RegexOptions.Compiled);
        private static readonly Regex MetricLabelNameRegex = new Regex("[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
        private static readonly Regex ReservedMetricLabelNameRegex = new Regex("__.*", RegexOptions.Compiled);

        protected readonly string _fullname;
        protected readonly string _help;
        protected readonly string[] _labelNames;

        private protected Collector(Builder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var name = builder.Name ?? throw new ArgumentNullException(nameof(builder.Name), "Name hasn't been set.");
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

            _help = builder.Help ?? throw new ArgumentNullException(nameof(builder.Help), "Help hasn't been set.");

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

        protected static void CheckMetricName(string name)
        {
            if (!MetricNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric name: " + name);
            }
        }

        protected static void CheckMetricLabelName(String name)
        {
            if (!MetricLabelNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric label name: " + name);
            }
            if (!ReservedMetricLabelNameRegex.IsMatch(name))
            {
                throw new ArgumentException("Invalid metric label name, reserved for internal use: " + name);
            }
        }

        public abstract class Builder
        {
            public string Name { get; private set; } = string.Empty;

            public string Namespace { get; private set; } = string.Empty;

            public string Subsystem { get; private set; } = string.Empty;

            public string Help { get; private set; } = string.Empty;

            public string[] LabelNames { get; private set; } = Array.Empty<string>();

            /// <summary>
            /// Set the name of the metric. Required.
            /// </summary>
            public Builder WithName(string name)
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
            public Builder WithNamespace(string ns)
            {
                Namespace = (ns ?? string.Empty).Trim();
                return this;
            }

            /// <summary>
            /// Sets an optional subsystem name.
            /// </summary>
            public Builder WithSubsystem(string subsystem)
            {
                Subsystem = (subsystem ?? string.Empty).Trim();
                return this;
            }

            /// <summary>
            /// Set the help string of the metric. Required.
            /// </summary>
            public Builder WithHelp(string help)
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
            public Builder WithLabels(params string[] labelNames)
            {
                LabelNames = labelNames ?? throw new ArgumentNullException(nameof(labelNames));
                return this;
            }
        }
    }

    public abstract class Collector<TChild> : Collector
        where TChild : new()
    {
        protected readonly ConcurrentDictionary<string[], TChild> _children =
            new ConcurrentDictionary<string[], TChild>(LabelArrayEqualityComparer.Default);

        protected TChild _noLabelsChild;

        private protected Collector(Builder builder) : base(builder) { }

        /// <summary>
        /// Return the Child with the given labels, creating it if needed.
        /// </summary>
        /// <remarks>
        /// Must be passed the same number of labels are were passed to {@link #labelNames}.
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
            return _children.GetOrAdd(labelValues, _ => new TChild());
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

        protected List<MetricFamilySamples> FamilySamplesList(CollectorType type, List<MetricFamilySamples.Sample> samples) =>
            new List<MetricFamilySamples>(1)
            {
               new MetricFamilySamples(_fullname, type, _help, samples),
            };

        protected void InitializeNoLabelsChild()
        {
            // Initialize metric if it has no labels.
            if (_labelNames.Length == 0)
            {
                _noLabelsChild = Labels();
            }
        }

        public abstract class Builder<TCollector> : Builder
            where TCollector : Collector<TChild>
        {
            protected abstract TCollector Create();

            /// <summary>
            /// Register the Collector with the default registry.
            /// </summary>
            public Collector<TChild> Register() => Register(CollectorRegistry.DefaultRegistry);

            /// <summary>
            /// Register the Collector with the given registry.
            /// </summary>
            public Collector<TChild> Register(CollectorRegistry registry)
            {
                var collector = Create();
                registry.Register(collector);
                return collector;
            }
        }
    }
}