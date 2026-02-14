using Commons.Configuration;
using Commons.Services.Interfaces;
using Confluent.Kafka;

namespace Consumer;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    readonly IConsumer<string, string> _consumer;
    readonly KafkaSettings _kafkaSettings;
    readonly ILogger<Worker> _logger;
    readonly IKafkaConsumerService _kafkaConsumerService;

    public Worker(ILogger<Worker> logger, IConsumer<string, string> consumer, KafkaSettings kafkaSettings,
        IKafkaConsumerService kafkaConsumerService) : this(logger)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kafkaConsumerService = kafkaConsumerService ?? throw new ArgumentNullException(nameof(kafkaConsumerService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_kafkaSettings.TopicName)
            ;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _kafkaConsumerService.ConsumeEvent(_consumer, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
