using System.Data;
using System.Net.Http.Headers;
using System.Reflection;
using System.Xml.Linq;

namespace common
{
    public class LogDataKafka
    {
        public static int MaxMessageLen = 10 * 1024 * 1024; // the max length of message
        public int level { get; set; }
        public string? logger { get; set; }
        public string? thread { get; set; }
        public DateTime log_time { get; set; }

        private string? _message;
        public string? message { get
            {
                return _message;
            }
            set
            {
                if (value?.Length > MaxMessageLen)
                    throw new InvalidOperationException($"Message length cannot exceed {MaxMessageLen}");
                _message = value;
            } }
    }
    public class LogDataKusto
    {
        const int kustoStringThreshhold = 1024 * 1024;
        public static string[] MsgColumns =
        {
            "message",
            "message_2",
            "message_3",
            "message_4",
            "message_5",
            "message_6",
            "message_7",
            "message_8",
            "message_9",
            "message_10",
            "message_11",
            "message_12",
            "message_13",
            "message_14",
            "message_15",
            "message_16"
        };
        public int level { get; set; }
        public string? logger { get; set; }
        public string? thread { get; set; }
        public DateTime log_time { get; set; }
        public string? message { get; set; } = "";
        public string? message_2 { get; set; } = "";
        public string? message_3 { get; set; } = "";
        public string? message_4 { get; set; } = "";
        public string? message_5 { get; set; } = "";
        public string? message_6 { get; set; } = "";
        public string? message_7 { get; set; } = "";
        public string? message_8 { get; set; } = "";
        public string? message_9 { get; set; } = "";
        public string? message_10 { get; set; } = "";
        public string? message_11 { get; set; } = "";
        public string? message_12 { get; set; } = "";
        public string? message_14 { get; set; } = "";
        public string? message_15 { get; set; } = "";
        public string? message_16 { get; set; } = "";
        public DateTime inserted_time { get; set; } = DateTime.Now;

        public LogDataKusto(LogDataKafka data)
        {
            level = data.level;
            logger= data.logger;
            thread = data.thread;
            log_time = data.log_time;
            // split the message into 1MB chunk
            if(data.message !=null)
            { // the string max length in kusto is 1MB. so we split the large message into 1MB chunks.
                int loop = data.message.Length % kustoStringThreshhold > 0 ? 
                    data.message.Length / kustoStringThreshhold + 1 :
                    data.message.Length / kustoStringThreshhold;
                for (int i = 0; i < loop; i++)
                {
                    string fieldName = MsgColumns[i];
                    int kustMsgLen = (i + 1) * kustoStringThreshhold > data.message.Length ?
                         data.message.Length - i * kustoStringThreshhold :
                         kustoStringThreshhold;

                    string value = data.message.Substring(i * kustoStringThreshhold,
                        kustMsgLen);
                   SetValue(fieldName, value);
                }
            }
        }
        public  void SetValue(string propertyName, object value)
        {
            var propertyInfo =GetType().GetProperty(propertyName);

            if (propertyInfo is null) return;

            var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            if (propertyInfo.PropertyType.IsEnum)
            {
                propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, value.ToString()!));
            }
            else
            {
                var safeValue = (value == null) ? null : Convert.ChangeType(value, type);
                propertyInfo.SetValue(this, safeValue, null);
            }
        }

    }
}