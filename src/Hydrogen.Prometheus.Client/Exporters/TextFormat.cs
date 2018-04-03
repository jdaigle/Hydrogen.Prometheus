using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hydrogen.Prometheus.Client.Internal;

namespace Hydrogen.Prometheus.Client.Exporters
{
    /// <summary>
    /// Implements version 0.0.4 of the Text Exposition Format
    /// </summary>
    /// <remarks>
    /// See http://prometheus.io/docs/instrumenting/exposition_formats/
    /// for the output format specification.
    /// </remarks>
    public static class TextFormat
    {
        /// <summary>
        /// The HTTP Content Type for version 0.0.4 of the Text Exposition Format
        /// </summary>
        public const string ContentType = "text/plain; version=0.0.4; charset=utf-8";

        private static readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Asynchronously writes the specified metric families to the
        /// specified output stream as plain text.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="metricFamilies">An enumerable of the metric families to write.</param>
        public static async Task WriteAsync(Stream stream, IEnumerable<MetricFamilySamples> metricFamilies)
        {
            if (stream == null)
            {
                throw new System.ArgumentNullException(nameof(stream));
            }

            if (metricFamilies == null)
            {
                throw new System.ArgumentNullException(nameof(metricFamilies));
            }

            using (var streamWriter = new StreamWriter(stream, _encoding))
            {
                streamWriter.NewLine = "\n";
                foreach (var metricFamily in metricFamilies)
                {
                    await WriteAsync(streamWriter, metricFamily);
                }
            }
        }

        private static async Task WriteAsync(StreamWriter writer, MetricFamilySamples metricFamily)
        {
            // # HELP familyname helptext
            await writer.WriteAsync("# HELP ");
            await writer.WriteAsync(metricFamily.Name);
            await writer.WriteAsync(" ");
            await WriteEscapedHelpAsync(writer, metricFamily.Help);

            // # TYPE familyname type
            await writer.WriteAsync("# TYPE ");
            await writer.WriteAsync(metricFamily.Name);
            await writer.WriteAsync(" ");
            await writer.WriteLineAsync(GetTypeString(metricFamily.Type));

            foreach (var sample in metricFamily.Samples)
            {
                // metric_name [
                //   "{" label_name "=" `"` label_value `"` { "," label_name "=" `"` label_value `"` } [ "," ] "}"
                // ] value [ timestamp ]
                await writer.WriteAsync(sample.Name);
                if (sample.LabelNames.Count > 0)
                {
                    await writer.WriteAsync('{');
                    for (int i = 0; i < sample.LabelNames.Count; ++i)
                    {
                        await writer.WriteAsync(sample.LabelNames[i]);
                        await writer.WriteAsync("=\"");
                        await WriteEscapedLabelValueAsync(writer, sample.LabelValues[i]);
                        await writer.WriteAsync("\",");
                    }
                    await writer.WriteAsync('}');
                }
                await writer.WriteAsync(' ');
                await writer.WriteAsync(sample.Value.ConvertToGoString());
                await writer.WriteAsync('\n');
            }
        }

        private static async Task WriteEscapedLabelValueAsync(StreamWriter writer, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                switch (c)
                {
                    case '\\':
                        await writer.WriteAsync("\\\\");
                        break;
                    case '\"':
                        await writer.WriteAsync("\\\"");
                        break;
                    case '\n':
                        await writer.WriteAsync("\\n");
                        break;
                    default:
                        await writer.WriteAsync(c);
                        break;
                }
            }
        }

        private static async Task WriteEscapedHelpAsync(StreamWriter writer, string help)
        {
            for (var i = 0; i < help.Length; i++)
            {
                var c = help[i];
                switch (c)
                {
                    case '\\':
                        await writer.WriteAsync("\\\\");
                        break;
                    case '\n':
                        await writer.WriteAsync("\\n");
                        break;
                    default:
                        await writer.WriteAsync(c);
                        break;
                }
            }
        }

        private static string GetTypeString(CollectorType type)
        {
            switch (type)
            {
                case CollectorType.Counter:
                    return "counter";
                case CollectorType.Gauge:
                    return "gauge";
                case CollectorType.Histogram:
                    return "histogram";
                case CollectorType.Summary:
                    return "summary";
                default:
                    return "untyped";
            }
        }
    }
}
