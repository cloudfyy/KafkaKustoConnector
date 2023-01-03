
# Kafka and Azure Data Explorer(Kusto) Connector

## Introduction

This tool can read messages from kafka and ingest them into Azure Data Explorer.

## Prerequisite

1. Kafka 2.4.1 or above
2. Azure Function
3. Azure Data Explorer
4. Azure Blob Stroage (Optional)
5. Azure Subscription

## Feature

1. Large Message (max size: 3.96MB)
2. Custom Metrics
3. Dropped Message Logging
4. Scale Out and in

## Project Description

### common

A common module project.

### KafkaKustoConnector

This project is an azure function. It use Kafka trigger to fectch message and send data to azure data explorer. It check message sizes. If the message szie is larger than the threshhold, it drops the message. All dropped messages is stored in a blob container.

It produce the follwing metrics  to azure application insights:

| Metric | Description |
| ----------- | ----------- |
| MessageReceived | The count of received messages  |
| MessageDropped | The count of dropped messages. All messages which size are larger than the threshold will be dropped. |
| MessageSentToADX | The cound of messages which are sent to azure data explorer. |
| BytesReceived | The bytes count which is received from Kafka |
| BytesSentToADX | The bytes count which is sent to azure data explorer |
| BytesDropped | The bytes count which is dropped by this tool |


### consumer

This project is a kafka test  cosumer. It can fetch large message.

```c#
var config = new ConsumerConfig
            {
                ...
                FetchMaxBytes = 10485760               
            };
```

### producer

This project is a test kafka producer. It supports larage message as well. 

This is an example to run producer:

``` bash
dotnet run -- 10.4.0.7:9092,10.4.0.6:9092,10.4.0.5:9092,10.4.0.4:9092 largemessage 100 10
```

- argument 0: kafka broker server list
- argument 1: kafka topic name
- argument 2: the message field length of LogData. because the LogData struc has  has other fileds. So the real "message" size will larger that this length. You need to add 100 bytes overhead for the size at last.
- argument 3: total message count

### KustoTest

This is a kusto ingest test project.

### KustoReader

A test project. It reads data from an ADX table.

### KafkaKustoConnector

Azure fuction connects a Kafka topic to an ADX table. 

### Configuration

| Config | Description |
| ----------- | ----------- |
|BOOTSTRAP_SERVERS | The bootstrap server list of Kafka. for example: 10.4.0.7:9092,10.4.0.6:9092,10.4.0.5:9092,10.4.0.4:9092  |
|Topic | The Kafka topic name|
|log | The log container user. It uses azure function default storage account.|
|KUSTO_INGEST_URI | ADX ingest Url. You can get it from the overview page of ADX |
|KUSTO_DB | ADX DB name |
|KUSTO_TABLE | ADX table name |
|KUSTO_JSON_MAPPING| ADX json mapping name |

Pleaser refer to this link for how to configure azure function:

https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal