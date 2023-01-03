using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaKustoConnector
{
    public interface ITelemetryProvider
    {
        void TrackDependency(string name, string typeName,
             string data, DateTime startTime, TimeSpan duration, bool success);
        void TrackEvent(string name);
        void TrackEvent(string name, Dictionary<string, string> properties);
        void TrackEvent(string name, Dictionary<string, string> properties,
                        Dictionary<string, double> metrics);
        void TrackException(Exception ex);
        void TrackMetric(string name, double value);
        Metric GetMetric(string metricId);
    }

    public class TelemetryProvider : ITelemetryProvider
    {
        private TelemetryClient _client;

        public TelemetryProvider()
        {
            _client = new TelemetryClient(TelemetryConfiguration.CreateDefault());
        }
        public void TrackEvent(string name)
        {
            _client.TrackEvent(name);
        }

        public void TrackEvent(string name, Dictionary<string, string> properties)
        {
            _client.TrackEvent(name, properties);
        }

        public void TrackEvent(string name,
            Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            _client.TrackEvent(name, properties, metrics);
        }

        public void TrackMetric(string name, double value)
        {
            _client.TrackMetric(name, value);
        }

        public void TrackException(Exception ex)
        {
            _client.TrackException(ex);
        }

        public void TrackDependency(string name, string typeName,
               string data, DateTime startTime, TimeSpan duration, bool success)
        {
            _client.TrackDependency(typeName, name, data, startTime, duration, success);
        }

        public Metric GetMetric(string metricId)
        {
            return _client.GetMetric(metricId);
        }
    }
}
