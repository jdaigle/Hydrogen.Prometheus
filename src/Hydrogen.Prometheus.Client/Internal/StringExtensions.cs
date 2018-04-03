namespace Hydrogen.Prometheus.Client.Internal
{
    /// <summary>
    /// Various string helper methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a double value into the Go language compatible wire format.
        /// </summary>
        /// <param name="d">The value to convert</param>
        public static string ConvertToGoString(this double d)
        {
            if (d == double.PositiveInfinity)
            {
                return "+Inf";
            }
            if (d == double.NegativeInfinity)
            {
                return "-Inf";
            }
            if (double.IsNaN(d))
            {
                return "NaN";
            }
            return d.ToString();
        }
    }
}
