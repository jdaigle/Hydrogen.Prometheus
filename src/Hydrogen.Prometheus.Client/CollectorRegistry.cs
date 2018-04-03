using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// A registry of Collectors.
    /// </summary>
    /// <remarks>
    /// The majority of users should use the <see cref="DefaultRegistry"/>, rather than instantiating their own.
    ///
    /// Creating a registry other than the default is primarily useful for unittests, or
    ///  pushing a subset of metrics to the<a href="https://github.com/prometheus/pushgateway">Pushgateway</a>
    /// from batch jobs.
    /// </remarks>
    public class CollectorRegistry
    {
        /// <summary>
        /// The default registry.
        /// </summary>
        public static readonly CollectorRegistry DefaultRegistry = new CollectorRegistry();

        private readonly Dictionary<Collector, IList<string>> _collectorToNames = new Dictionary<Collector, IList<string>>();
        private readonly Dictionary<string, Collector> _namesToCollectors = new Dictionary<string, Collector>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register a Collector.
        /// </summary>
        /// <param name="collector"></param>
        public void Register(Collector collector)
        {
            var names = CollectorNames(collector);
            lock (_collectorToNames)
            {
                foreach (var name in names)
                {
                    if (_namesToCollectors.ContainsKey(name))
                    {
                        throw new ArgumentException("Collector already registered that provides name: " + name);
                    }
                }
                foreach (var name in names)
                {
                    _namesToCollectors[name] = collector;
                }
                _collectorToNames[collector] = names;
            }
        }

        /// <summary>
        /// Unregister a Collector.
        /// </summary>
        /// <param name="collector"></param>
        public void Unregister(Collector collector)
        {
            lock (_collectorToNames)
            {
                if (_collectorToNames.TryGetValue(collector, out var names))
                {
                    _collectorToNames.Remove(collector);
                    foreach (var name in names)
                    {
                        _namesToCollectors.Remove(name);
                    }
                }
            }
        }

        /// <summary>
        /// Enumeration of metrics of all registered collectors.
        /// </summary>
        public IEnumerable<MetricFamilySamples> MetricFamilySamples() => _collectorToNames.Keys.ToList().SelectMany(x => x.Collect());

        private static IList<string> CollectorNames(Collector collector)
        {
            var name = collector.Name;
            switch (collector)
            {
                case Histogram historgram:
                    return new string[] { name + "_count", name + "_sum", name + "_bucket", name };
                default:
                    return new string[] { name };
            }
        }
    }
}
