namespace Hydrogen.Prometheus.Client.Internal
{
    public class StringHelpers
    {
        public static string DoubleToGoString(double d)
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
