using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydrogen.Prometheus.Client
{
    public class CollectorRegistry
    {
        public static readonly CollectorRegistry DefaultRegistry = new CollectorRegistry();

        public void Register(Collector collector)
        {

        }

        public void Unregister(Collector collector)
        {

        }
    }
}
