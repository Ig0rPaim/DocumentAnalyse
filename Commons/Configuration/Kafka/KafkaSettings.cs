using Confluent.Kafka;

namespace Commons.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string PostFileTopicName { get; set; } = string.Empty;
    public string ProcessedFileTopicName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public int AutoOffsetReset { get; set; } = 1;
    public bool EnableAutoCommit { get; set; } = false;
    public string KeyType { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string ConsumerReturn { get; set; } = string.Empty;
}
