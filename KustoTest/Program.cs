using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Ingestion;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace KustoTest
{
    internal class Program
    {
        private const string authority = "6bdfaeb3-c962-41e7-9107-7a6b2715e402";
        private const string applicationClientId = "6dc264f3-a926-4577-9f5e-f5cf1c30777d";
        private const string applicationKey = "7zW8Q~ChJ0Jh5ZwBdh4jxUV7mdN_cC6~FY3fHdkr";
        private const string s_jsonMappingName = "m_k2_logs";
        //const int msgLen = 1048576 * 3 + 512 * 1024 + 256 * 1024 + 128 * 1024 + 64 * 1024 + 32 * 1024  ;
        const int msgLen = 1048576*5+110;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Kusto test app starting...");
            if (!ParseCommandLine(args, out var cluster, out var database, out var table, out var mode))
            {
                Usage();
                return;
            }

            Uri clusterUri = new Uri(cluster);
            string ingestUrl = $"{clusterUri.Scheme}://ingest-{clusterUri.Host}/";

            KustoConnectionStringBuilder kcsb = null;
            if(mode.ToLower() == "queue")
            {
                kcsb = new KustoConnectionStringBuilder
                {
                    DataSource = ingestUrl
                };
            }
            else
            {
                kcsb = new KustoConnectionStringBuilder
                {
                    DataSource = cluster
                };
            }
            
            kcsb = kcsb.WithAadApplicationKeyAuthentication(applicationClientId,
                applicationKey, authority);
           
            if ( mode.ToLower()=="queue" )
            {
                Console.WriteLine("queue mode...");
                // Do ingestion using Kusto.Data client library 
                using (var siClient = KustoIngestFactory.CreateQueuedIngestClient(kcsb))
                {
                    using (var data = CreateSampleEventLogJsonStream(1, msgLen))
                    {
                        var kustoIngestionProperties = new KustoIngestionProperties(database, table)
                        {
                            Format = DataSourceFormat.json,
                            IngestionMapping = new IngestionMapping()
                            {
                                IngestionMappingReference = s_jsonMappingName,
                                IngestionMappingKind = IngestionMappingKind.Json
                            },
                        };
                        await siClient.IngestFromStreamAsync(
                            data,
                            kustoIngestionProperties
                           ); // In real code use await in async method
                    }
                }
            }
            else
            {
                Console.WriteLine("stream mode...");
                using (var siClient = KustoClientFactory.CreateCslStreamIngestClient(kcsb))
                {
                    using (var data = CreateSampleEventLogJsonStream(1, msgLen))
                    {
                        await siClient.ExecuteStreamIngestAsync(
                            database,
                            table,
                            data,
                            null,
                            DataSourceFormat.json,
                             compressStream: false,
                             s_jsonMappingName
                           ); // In real code use await in async method
                    }
                }
            }
        }

        private static Stream CreateSampleEventLogJsonStream(int numberOfRecords, int messageLen)
        {
            var ms = new MemoryStream();
            using (var tw = new StreamWriter(ms, Encoding.UTF8, 4096, true))
            {
                for (int i = 0; i < numberOfRecords; i++)
                {
                    var dataKafka = new common.LogDataKafka
                    {
                        level = 0,
                        logger = "log4j",
                        thread = $"thread{i}",
                        log_time = DateTime.Now,
                    };
                    //  try serialize it got the length
                    var emptyMsgJsonPayload = JsonSerializer.Serialize(dataKafka);
                    // get message field length
                    if (messageLen > emptyMsgJsonPayload.Length)
                    {
                        dataKafka.message = RandomString(messageLen - emptyMsgJsonPayload.Length);
                    }

                    // covert message to kusto format
                    var kustData = new common.LogDataKusto(dataKafka);

                    string result = JsonSerializer.Serialize(kustData);
                    Console.WriteLine($"json length:{result.Length}");
                    tw.WriteLine(result);
                    //payloads.Add(payload);
                }
            }
            ms.Seek(0, SeekOrigin.Begin);

            Console.WriteLine($"payload length: {ms.Length}");
            return ms;
        }
        static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\\KustoTest -cluster <cluster url> -db <database> -table <table> -mode stream|queue");
        }
        static bool ParseCommandLine(string[] args, out string cluster, out string database, out string table, out string mode)
        {
            cluster = null;
            database = null;
            table = null;
            mode = null;
            int i = 0;

            while (i < args.Length)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "-cluster":
                        cluster = args[++i];
                        break;
                    case "-db":
                        database = args[++i];
                        break;
                    case "-table":
                        table = args[++i];
                        break;
                    case "-mode":
                        mode = args[++i];
                        break;
                    default:
                        Console.Error.WriteLine($"Unrecognized argument {args[i]}");
                        return false;
                }
                i++;
            }

            if (String.IsNullOrEmpty(cluster))
            {
                Console.Error.WriteLine("Cluster missing");
                return false;
            }

            if (String.IsNullOrEmpty(database))
            {
                Console.Error.WriteLine("Database missing");
                return false;
            }

            if (String.IsNullOrEmpty(table))
            {
                Console.Error.WriteLine("Table missing");
                return false;
            }

            if (String.IsNullOrEmpty(mode))
            {
                Console.Error.WriteLine("mode missing");
                return false;
            }

            return true;
        }

        static string RandomString(int length)
        {
            var chars = "0123456789abcdefghijklmnopqrstuvwxyz!@#$%";
            var output = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                output.Append(chars[random.Next(chars.Length)]);
            }
            return output.ToString();
        }

    }
}