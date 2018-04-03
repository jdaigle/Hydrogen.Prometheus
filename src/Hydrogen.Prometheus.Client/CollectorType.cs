namespace Hydrogen.Prometheus.Client
{
    /// <summary>
    /// Represents the built-in metric types.
    /// </summary>
    public enum CollectorType
    {
        /// <summary>
        /// A <see cref="Hydrogen.Prometheus.Client.Counter"/> metric.
        /// </summary>
        Counter,

        /// <summary>
        /// A <see cref="Hydrogen.Prometheus.Client.Gauge"/> metric.
        /// </summary>
        Gauge,

        /// <summary>
        /// A <see cref="Hydrogen.Prometheus.Client.Histogram"/> metric.
        /// </summary>
        Histogram,

        /// <summary>
        /// TODO
        /// </summary>
        Summary,
    }
}
