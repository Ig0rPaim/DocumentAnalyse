using Commons.Models;

namespace Commons.Services.Interfaces;

public interface IKafkaProducerService
{
    public Task ProduceEvent(string topic, Event @event);
}

public interface IKafkaProducerService<TKey, TValue> : IKafkaProducerService
{
    public Task ProduceEvent(string topic, Event @event);
}