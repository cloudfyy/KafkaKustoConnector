.alter database k2logs policy streamingingestion 
'{"IsEnabled": true, "HintAllocatedRate": 2.1}'

.alter database k2logs policy ingestionbatching
```
{
    "MaximumBatchingTimeSpan" : "00:00:30",
    "MaximumNumberOfItems" : 500,
    "MaximumRawDataSizeMB" : 1024
}
```

# stream ingestion policy
.alter database k2logs policy streamingingestion enable

.clear table t_logs data
.clear table t_test1 data

.show cluster policy managed_identity
.show database k2logs policy managed_identity

.alter database k2logs policy managed_identity ```
[
  {
    "ObjectId": "5e987d86-a3c8-4577-8fcf-2f23b3ee963c",
    "AllowedUsages": "NativeIngestion"
  }
]
```

.create-merge table t_logs (['records']:dynamic)  


.create-or-alter table t_logs ingestion json mapping
"m_logs"
'['
'{"column":"records","path":"$","datatype":"","transform":null}'
']';

.create-or-alter table t_test1 ingestion json mapping

"m_test1"

'['

'    { "column" : "level", "datatype" : "string", "Properties":{"Path":"$.level"}},'

'    { "column" : "logger", "datatype" : "string", "Properties":{"Path":"$.logger"}},'

'    { "column" : "thread", "datatype" : "string", "Properties":{"Path":"$.thread"}},'

'    { "column" : "log_time", "datatype" : "datetime", "Properties":{"Path":"$.time"}},'

'    { "column" : "message", "datatype" : "string", "Properties":{"Path":"$.message"}},'

'    { "column" : "inserted_time", "datatype" : "datetime", "Properties":{"Path":"$.inserted_time"}}'

']'


.create-merge table t_k2_logs_dhj
(level:string,
logger:string,
thread:string,
log_time:datetime,
message:string,
message_2:string,
message_3:string,
message_4:string,
message_5:string,
message_6:string,
message_7:string,
message_8:string,
message_9:string,
message_10:string,
message_11:string,
message_12:string,
message_13:string,
message_14:string,
message_15:string,
message_16:string,
inserted_time:datetime )


.create-or-alter table t_k2_logs_dhj ingestion json mapping
"m_k2_logs"
'['
'    { "column" : "level", "datatype" : "string", "Properties":{"Path":"$.level"}},'
'    { "column" : "logger", "datatype" : "string", "Properties":{"Path":"$.logger"}},'
'    { "column" : "thread", "datatype" : "string", "Properties":{"Path":"$.thread"}},'
'    { "column" : "log_time", "datatype" : "datetime", "Properties":{"Path":"$.log_time"}},'
'    { "column" : "message", "datatype" : "string", "Properties":{"Path":"$.message"}},'
'    { "column" : "message_2", "datatype" : "string", "Properties":{"Path":"$.message_2"}},'
'    { "column" : "message_3", "datatype" : "string", "Properties":{"Path":"$.message_3"}},'
'    { "column" : "message_4", "datatype" : "string", "Properties":{"Path":"$.message_4"}},'
'    { "column" : "message_5", "datatype" : "string", "Properties":{"Path":"$.message_5"}},'
'    { "column" : "message_6", "datatype" : "string", "Properties":{"Path":"$.message_6"}},'
'    { "column" : "message_7", "datatype" : "string", "Properties":{"Path":"$.message_7"}},'
'    { "column" : "message_8", "datatype" : "string", "Properties":{"Path":"$.message_8"}},'
'    { "column" : "message_9", "datatype" : "string", "Properties":{"Path":"$.message_9"}},'
'    { "column" : "message_10", "datatype" : "string", "Properties":{"Path":"$.message_10"}},'
'    { "column" : "message_11", "datatype" : "string", "Properties":{"Path":"$.message_11"}},'
'    { "column" : "message_12", "datatype" : "string", "Properties":{"Path":"$.message_12"}},'
'    { "column" : "message_13", "datatype" : "string", "Properties":{"Path":"$.message_13"}},'
'    { "column" : "message_14", "datatype" : "string", "Properties":{"Path":"$.message_14"}},'
'    { "column" : "message_15", "datatype" : "string", "Properties":{"Path":"$.message_15"}},'
'    { "column" : "message_16", "datatype" : "string", "Properties":{"Path":"$.message_16"}},'
'    { "column" : "inserted_time", "datatype" : "datetime", "Properties":{"Path":"$.inserted_time"}}'
']'

 t_k2_logs

| extend message=strcat(message,message_2,message_3,message_4,message_5,message_6,message_7,message_8,message_9,message_10)

| project level, logger, thread, log_time, message
