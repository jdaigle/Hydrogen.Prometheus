using System.Collections.Generic;
using System.Diagnostics;

namespace Hydrogen.Prometheus.Client.Internal
{
    /// <summary>
    /// Compares label arrays for equality.
    /// </summary>
    public class LabelArrayEqualityComparer : IEqualityComparer<string[]>
    {
        /// <summary>
        /// The default label array comparer.
        /// </summary>
        public static readonly LabelArrayEqualityComparer Default = new LabelArrayEqualityComparer();

        private LabelArrayEqualityComparer() { }

        /// <summary>
        /// Determines whether the specified label arrays are equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
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

        /// <summary>
        /// Returns a hash code for the specified label array.
        /// </summary>
        /// <param name="obj"></param>
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
