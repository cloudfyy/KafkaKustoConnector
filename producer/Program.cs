using common;
using Confluent.Kafka;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;


namespace producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Configure the client with credentials for connecting to Confluent.
            // Don't do this in production code. For more information, see 
            // https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets.
            var clientConfig = new ProducerConfig();
            clientConfig.BootstrapServers = args[0];
            clientConfig.SecurityProtocol = SecurityProtocol.Plaintext;
            //clientConfig.MaxReqestSize = 10485760;
            clientConfig.MessageMaxBytes = 10485880;
            //clientConfig.SslCaLocation = @"C:\work\vanke\PoC\kafkaclient\producer\ca.crt";
            //clientConfig.SecurityProtocol = Confluent.Kafka.SecurityProtocol.SaslSsl;
            //clientConfig.SaslMechanism = Confluent.Kafka.SaslMechanism.Plain;
            //clientConfig.SaslUsername = "<api-key>";
            //clientConfig.SaslPassword = "<api-secret>";
            //clientConfig.SslCaLocation = "probe"; // /etc/ssl/certs
            int message_len = int.Parse(args[2]);
            await Produce(args[1], clientConfig, message_len, int.Parse(args[3]));


            //Consume("recent_changes", clientConfig);

            Console.WriteLine("Exiting");
        }

        // Produce recent-change messages from Wikipedia to a Kafka topic.
        // The messages are sent from the RCFeed https://www.mediawiki.org/wiki/Manual:RCFeed
        // to the topic with the specified name. 
        static async Task Produce(string topicName, ClientConfig config, int msglen, int loop)

        {

            // Declare the producer reference here to enable calling the Flush
            // method in the finally block, when the app shuts down.
            IProducer<string, string> producer = null;

            try
            {
                // Build a producer based on the provided configuration.
                // It will be disposed in the finally block.
                producer = new ProducerBuilder<string, string>(config).Build();
                for(int i =0; i<loop; i++)
                {
                    var payLoad = new common.LogDataKafka
                    {
                        level = 0,
                        logger = "log4j",
                        thread = $"thread{i}",
                        log_time = DateTime.Now,
                        message = ""
                    };
                    //  try serialize it got the length
                    var emptyMsgJsonPayload = JsonSerializer.Serialize(payLoad);
                    // get message field length
                    if (msglen > emptyMsgJsonPayload.Length)
                    {
                        payLoad.message = RandomString(msglen - emptyMsgJsonPayload.Length);
                    }
                    var kustoFormatData = new LogDataKusto(payLoad);
                    var jsonPayload = JsonSerializer.Serialize(kustoFormatData);
                    Console.WriteLine($"level: {kustoFormatData.level}, logger: {kustoFormatData.logger}, " +
                        $"thread: {kustoFormatData.thread}, log_time:{kustoFormatData.logger}" +
                        $"message len: {kustoFormatData.message?.Length},  message 2 len: {kustoFormatData.message_2?.Length}");
                    producer.Produce(topicName,
                            new Message<string, string> { Key = $"key{i}", Value = jsonPayload },
                            (deliveryReport) =>
                            {
                                if (deliveryReport.Error.Code != ErrorCode.NoError)
                                {
                                    Console.WriteLine($"Failed to deliver message:          {deliveryReport.Error.Reason}");
                                }
                                else
                                {
                                    Console.WriteLine($"Produced message to: {deliveryReport.TopicPartitionOffset}");
                                }
                            }
                        );
                }
                

            }
            finally
            {
                var queueSize = producer.Flush(TimeSpan.FromSeconds(5));
                if (queueSize > 0)
                {
                    Console.WriteLine("WARNING: Producer event queue has " + queueSize + " pending events on exit.");
                }
                producer.Dispose();
            }
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