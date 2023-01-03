using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace producer
{
    internal class ProducerConfigMaxRequestSize : ProducerConfig
    {
        public int? MaxReqestSize
        {
            get
            {
                return GetInt("max.request.size");
            }
            set
            {
                SetObject("max.request.size", value);
            }
        }
    }
}
