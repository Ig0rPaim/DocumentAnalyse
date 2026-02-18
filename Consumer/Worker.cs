using System.Text.Json;
using Commons.Configuration;
using Commons.Models;
using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Confluent.Kafka;

namespace Consumer;

public class Worker(
    ILogger<Worker> logger,
    IConsumer<string, string> consumer,
    KafkaSettings kafkaSettings,
    IKafkaConsumerService<string, string> kafkaConsumerService,
    IKafkaProducerService<string, string> kafkaProducerService)
    : BackgroundService
{
    readonly IConsumer<string, string> _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
    readonly KafkaSettings _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
    readonly ILogger<Worker> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    readonly IKafkaConsumerService _kafkaConsumerService = kafkaConsumerService ?? throw new ArgumentNullException(nameof(kafkaConsumerService));
    readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_kafkaSettings.PostFileTopicName);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            { 
                var response = await _kafkaConsumerService.ConsumeEvent(_consumer, stoppingToken);
                
                if(response is null)
                    continue;

                await _kafkaProducerService.ProduceEvent(_kafkaSettings.ProcessedFileTopicName, response);

                _consumer.Commit();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
