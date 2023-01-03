using Azure.Storage.Blobs;
using common;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Ingestion;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaKustoConnector
{
    public class KafkaKustoConnector
    {
        private readonly ITelemetryProvider telemetryClient;
        const int LargeMsgThreshhold = 1024*1024;
        public KafkaKustoConnector(ITelemetryProvider telemetryClient)
        {
            this.telemetryClient = telemetryClient;
        }
       
        [FunctionName("DataIngestor")]
        public async Task Run(
            [KafkaTrigger("%BOOTSTRAP_SERVERS%",
                          "%TOPIC%",
                          Username = "user1", // not used
                          Password = "pwd", // not used
                          Protocol = BrokerProtocol.Plaintext,
                          AuthenticationMode = BrokerAuthenticationMode.Plain,
                          ConsumerGroup = "dotnet-kafka")] KafkaEventData<string>[] events,
                          [Blob("log")] BlobContainerClient containerLog,
                          ILogger log)
        {
            string bookstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            string topic = Environment.GetEnvironmentVariable("TOPIC");
            log.LogDebug($"BOOTSTRAP_SERVERS: {bookstrapServers}, TOPIC: {topic}");

            string ingestUrl = Environment.GetEnvironmentVariable("KUSTO_INGEST_URI");
            string database = Environment.GetEnvironmentVariable("KUSTO_DB");
            string table = Environment.GetEnvironmentVariable("KUSTO_TABLE");
            string jsonMapping = Environment.GetEnvironmentVariable("KUSTO_JSON_MAPPING");
            log.LogDebug($"KUSTO_INGEST_URI: {ingestUrl}, KUSTO_DB: {database}, " +
                $"KUSTO_TABLE: {table}, KUSTO_JSON_MAPPING: {jsonMapping}");

            telemetryClient.GetMetric("MessageReceived").TrackValue(events.Count());

            var kustoDataList = new List<LogDataKusto>();
            int bytesReceived = 0;
            foreach (KafkaEventData<string> eventData in events)
            {
                //log.LogTrace(eventData.Value);
                LogDataKafka kafkaData = JsonSerializer.Deserialize<LogDataKafka>(eventData.Value);
                kustoDataList.Add(new LogDataKusto(kafkaData));
                bytesReceived += eventData.Value.Length;

                if(eventData.Value.Length > LargeMsgThreshhold)
                    telemetryClient.GetMetric("LargeMessageReceived").TrackValue(1);
                // log the message to blob
                await UploadString(containerLog, GetBlobFileName(eventData), eventData.Value);
            }
            telemetryClient.GetMetric("BytesReceived").TrackValue(bytesReceived);

            var kcsb = new KustoConnectionStringBuilder
            {
                DataSource = ingestUrl
            };

            kcsb = kcsb.WithAadSystemManagedIdentity();
            using (var siClient = KustoIngestFactory.CreateQueuedIngestClient(kcsb))
            {
                using (var data = CreateJsonStreamData(kustoDataList, log))
                {
                    var kustoIngestionProperties = new KustoIngestionProperties(database, table)
                    {
                        Format = DataSourceFormat.json,
                        IngestionMapping = new IngestionMapping()
                        {
                            IngestionMappingReference = jsonMapping,
                            IngestionMappingKind = IngestionMappingKind.Json
                        },
                    };
                    long bytesSentToADX = data.Length; 
                    await siClient.IngestFromStreamAsync(
                        data,
                        kustoIngestionProperties
                       ); // In real code use await in async method
                    telemetryClient.GetMetric("MessageSentToADX").TrackValue(kustoDataList.Count());
                    telemetryClient.GetMetric("BytesSentToADX").TrackValue(bytesSentToADX);
                }
            }
        }

        private static Stream CreateJsonStreamData(List<LogDataKusto> data, ILogger log)
        {
            var ms = new MemoryStream();
            
            using (var tw = new StreamWriter(ms, Encoding.UTF8, 4096, true))
            {
                foreach(var s in data)
                {
                    //log.LogTrace(JsonSerializer.Serialize(s));
                    tw.WriteLine(JsonSerializer.Serialize(s));
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            log.LogDebug($"ADX payload length: {ms.Length}");
            return ms;
        }

        private async Task UploadString(BlobContainerClient containerClient, string filename, string content)
        {
            containerClient.CreateIfNotExists();
            BlobClient blobClient = containerClient.GetBlobClient(filename);
            await blobClient.UploadAsync(BinaryData.FromString(content), overwrite: true);
        }

        private string GetBlobFileName(KafkaEventData<string> eventData)
        {
            return $"{eventData.Topic}-{eventData.Partition}-{eventData.Offset}-" +
                        $"{eventData.Timestamp.ToString("yyyyMMddhhmmss")}-{Guid.NewGuid()}.json";
        }
    }
}
