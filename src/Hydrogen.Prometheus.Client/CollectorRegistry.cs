using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Register a Collector.
        /// </summary>
        /// <param name="collector"></param>
        public void Register(Collector collector)
        {

        }

        /// <summary>
        /// Unregister a Collector.
        /// </summary>
        /// <param name="collector"></param>
        public void Unregister(Collector collector)
        {

        }
    }
}
