using System.Collections.Generic;
using System.Diagnostics;

namespace Hydrogen.Prometheus.Client.Internal
{
    public class LabelArrayEqualityComparer : IEqualityComparer<string[]>
    {
        public static readonly LabelArrayEqualityComparer Default = new LabelArrayEqualityComparer();

        private LabelArrayEqualityComparer() { }

        public bool Equals(string[] x, string[] y)
        {
            Debug.Assert(x != null);
            Debug.Assert(y != null);
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(string[] obj)
        {
            Debug.Assert(obj != null);
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i].GetHashCode();
                }
            }
            return result;
        }
    }
}
