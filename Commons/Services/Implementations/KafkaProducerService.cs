using Commons.Models;
using Commons.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Commons.Services.Implementations;

public class KafkaProducerService<TKey, TValue>(
    IProducer<TKey, TValue> producer,
    ILogger<KafkaProducerService<TKey, TValue>> logger,
    IEventService<TKey, TValue> eventService,
    ISerializatorService<TKey, TValue> serializatorService)
    : IKafkaProducerService<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    private readonly ILogger<KafkaProducerService<TKey, TValue>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IEventService<TKey, TValue> _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
    private readonly ISerializatorService<TKey, TValue> _serializatorService = serializatorService ?? throw new ArgumentNullException(nameof(serializatorService));

    // public Task ProduceEvent(string topic, Event @event)
    // {
    //     return ProduceEvent(topic, @event);
    //     
    // }

    public async Task ProduceEvent(string topic, Event @event)
    {
        try
        {
            var serializedKey = _serializatorService.SerializeKeyType(@event.FullName);
            var serializedValue = _serializatorService.SerializeValueType(@event);
            
            _eventService.KeyIsValid(serializedKey);
            _eventService.ValueIsValid(serializedValue);
            
            var kafkaMessage = new Message<TKey, TValue>
            {
                Key = serializedKey,
                Value = serializedValue
            };

            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage);
            _logger.LogInformation("Delivered message to {TopicPartitionOffset}", deliveryResult.TopicPartitionOffset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error producing event to topic {Topic}", topic);
            throw;
        }
    }
}