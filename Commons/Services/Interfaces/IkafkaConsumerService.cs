using Confluent.Kafka;

namespace Commons.Services.Interfaces;

public interface IKafkaConsumerService
{
    public Task ConsumeEvent(IConsumer<string, string> consumer, CancellationToken stoppingToken);
}