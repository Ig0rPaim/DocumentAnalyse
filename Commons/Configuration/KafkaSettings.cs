using Confluent.Kafka;

namespace Commons.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public int AutoOffsetReset { get; set; } = 1;
    public bool EnableAutoCommit { get; set; } = false;
}
