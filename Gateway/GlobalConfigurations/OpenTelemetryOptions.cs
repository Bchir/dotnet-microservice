using OpenTelemetry.Exporter;

namespace Gateway;

public static partial class OpenTelemetryIntegration
{
    public class OpenTelemetryOptions
    {
        public bool Enabled => Endpoint is not null;

        [ConfigurationKeyName("OTEL_EXPORTER_OTLP_ENDPOINT")]
        public Uri? Endpoint { get; set; }

        [ConfigurationKeyName("OTEL_EXPORTER_OTLP_HEADERS")]
        public string? Headers { get; set; }

        [ConfigurationKeyName("OTEL_EXPORTER_OTLP_TIMEOUT")]
        public int TimeoutMilliseconds { get; set; } = 10000;

        [ConfigurationKeyName("OTEL_EXPORTER_OTLP_PROTOCOL")]
        public OtlpExportProtocol Protocol { get; set; } = OtlpExportProtocol.Grpc;

        public IDictionary<string, string> ParsedHeaders => ParseStringToDictionary(Headers);

        private static Dictionary<string, string> ParseStringToDictionary(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return [];
            var dict = new Dictionary<string, string>();

            string[] pairs = input.Split(',');
            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split('=');
                dict.Add(keyValue[0], keyValue[1]);
            }
            return dict;
        }
    }
}