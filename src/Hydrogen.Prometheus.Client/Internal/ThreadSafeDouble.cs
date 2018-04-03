using System.Threading;

namespace Hydrogen.Prometheus.Client.Internal
{
    /// <summary>
    /// Helper methods for thread-safe double arithmetic.
    /// </summary>
    public static class ThreadSafeDouble
    {
        /// <summary>
        /// Increments a double value using an atomic compare-and-swap (CAS).
        /// </summary>
        /// <param name="value">A reference to value to increment.</param>
        /// <param name="inc">The amount the increment the value.</param>
        public static void Add(ref double value, double inc)
        {
            // note: requres 64bit architecture in order to work correctly
            double initialValue, incrementedValue;
            do
            {
                initialValue = value;
                incrementedValue = initialValue + inc;
            } while (initialValue != Interlocked.CompareExchange(ref value, incrementedValue, initialValue));
        }
    }
}
