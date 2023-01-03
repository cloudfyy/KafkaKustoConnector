using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

namespace KustoReader
{
    // This sample illustrates how to query Kusto using the Kusto.Data .NET library.
    //
    // For the purpose of demonstration, the query being sent retrieves multiple result sets.
    //
    // The program should execute in an interactive context (so that on first run the user
    // will get asked to sign in to Azure AD to access the Kusto service).
    class Program
    {
        const string Cluster = "https://k2adx.eastasia.kusto.windows.net";
        const string Database = "k2logs";
        const string table = "t_k2_logs";

        private const string authority = "6bdfaeb3-c962-41e7-9107-7a6b2715e402";
        private const string applicationClientId = "6dc264f3-a926-4577-9f5e-f5cf1c30777d";
        private const string applicationKey = "***";

        static void Main()
        {
            // The query provider is the main interface to use when querying Kusto.
            // It is recommended that the provider be created once for a specific target database,
            // and then be reused many times (potentially across threads) until it is disposed-of.
            var kcsb = new KustoConnectionStringBuilder(Cluster, Database)
                .WithAadApplicationKeyAuthentication(applicationClientId,
                applicationKey, authority);
            using (var queryProvider = KustoClientFactory.CreateCslQueryProvider(kcsb))
            {
                // The query -- Note that for demonstration purposes, we send a query that asks for two different
                // result sets (HowManyRecords and SampleRecords).
                var query = $"{table} | count | as HowManyRecords; {table} | limit 10 | extend message=strcat(message,message_2,message_3,message_4,message_5,message_6,message_7,message_8,message_9,message_10) | project level, logger, thread, log_time, message | as LogRecords";

                // It is strongly recommended that each request has its own unique
                // request identifier. This is mandatory for some scenarios (such as cancelling queries)
                // and will make troubleshooting easier in others.
                var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };
                using (var reader = queryProvider.ExecuteQuery(query, clientRequestProperties))
                {
                    // Read HowManyRecords
                    while (reader.Read())
                    {
                        var howManyRecords = reader.GetInt64(0);
                        Console.WriteLine($"There are {howManyRecords} records in the table");
                    }

                    // Move on to the next result set, SampleRecords
                    reader.NextResult();
                    Console.WriteLine();
                    while (reader.Read())
                    {
                        // Important note: For demonstration purposes we show how to read the data
                        // using the "bare bones" IDataReader interface. In a production environment
                        // one would normally use some ORM library to automatically map the data from
                        // IDataReader into a strongly-typed record type (e.g. Dapper.Net, AutoMapper, etc.)
                        string level = reader.GetString(0);
                        string logger = reader.GetString(1);
                        string thread = reader.GetString(2);
                        //DateTime log_time = reader.GetDateTime(3);
                        string message = reader.GetString(4);
                        Console.WriteLine($"{level},{logger},{thread},  {message.Length}");
                    }
                }
            }
        }
    }
}