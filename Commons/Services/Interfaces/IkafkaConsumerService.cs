using Commons.Models;
using Confluent.Kafka;

namespace Commons.Services.Interfaces;


public interface IKafkaConsumerService
{    
    public Task<Event> ConsumeEvent(object consumer, CancellationToken stoppingToken);
}

public interface IKafkaConsumerService<TKey, TValue> : IKafkaConsumerService
{
    public Task<Event> ConsumeEvent(IConsumer<TKey, TValue> consumer, CancellationToken stoppingToken);
}