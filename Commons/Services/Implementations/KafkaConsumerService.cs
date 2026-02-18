using System.Text.Json;
using Commons.Models;
using Commons.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Commons.Services.Implementations;

public class KafkaConsumerService<TKey, TValue>(
    ILogger<KafkaConsumerService<TKey, TValue>> logger,
    IProcessor processor,
    ISerializatorService<TKey, TValue> serializatorService)
    : IKafkaConsumerService<TKey, TValue>
{
    private readonly ILogger<KafkaConsumerService<TKey, TValue>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessor _processor = processor ?? throw new ArgumentNullException(nameof(processor));

    private readonly ISerializatorService<TKey, TValue> _serializatorService =
        serializatorService ?? throw new ArgumentNullException(nameof(serializatorService));

    async Task<Event> IKafkaConsumerService.ConsumeEvent(object consumer, CancellationToken stoppingToken)
    {
        return await ConsumeEvent((IConsumer<TKey, TValue>)consumer, stoppingToken);
    }

    public async Task<Event> ConsumeEvent(IConsumer<TKey, TValue> consumer, CancellationToken stoppingToken)
    {
        var (fileStream, fileName) = new ValueTuple<Stream, string>();
        try
        {
            var result = consumer.Consume(stoppingToken);

            if (result != null)
            {
                var @event = _serializatorService.DeserializeValueType<Event>(result.Message.Value);
                _logger.LogInformation($"Processing event: {@event?.FullName ?? "event name not found"}");

                return await _processor.Process(@event);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to process file.");
            throw;
        }
        finally
        {
            if (fileStream is not null) await fileStream.DisposeAsync();
        }
    }
}