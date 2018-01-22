using System.Threading;

namespace Hydrogen.Prometheus.Client.Internal
{
    public static class ThreadSafeDouble
    {
        internal static void Add(ref double value, double inc)
        {
            // note: requres 64bit architecture in order to work correctly
            double initialValue, incrementedValue;
            do
            {
                initialValue = value;
                incrementedValue = initialValue + inc;
            } while (initialValue != Interlocked.CompareExchange(ref value, incrementedValue, incrementedValue));
        }
    }
}
