using System.Text.Json;
using Commons.Models;
using Commons.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Commons.Services.Implementations;

public class KafkaConsumerService : IKafkaConsumerService
{
    readonly ILogger<KafkaConsumerService> _logger;
    readonly IDocumentProcessor _documentProcessor;
    readonly IMinIoService _minIoService;

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IDocumentProcessor documentProcessor,
        IMinIoService minIoService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
        _minIoService = minIoService ?? throw new ArgumentNullException(nameof(minIoService));
    }

    public async Task ConsumeEvent(IConsumer<string, string> consumer, CancellationToken stoppingToken)
    {
        try
        {
            var result = consumer.Consume(stoppingToken);
                
            if (result != null)
            {
                var documentEvent = JsonSerializer.Deserialize<DocumentEvent>(result.Message.Value);
                _logger.LogInformation($"Processando arquivo: {documentEvent.ObjectName}");

                await _documentProcessor.Process(await _minIoService.Get(documentEvent.ObjectName));

                consumer.Commit(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar documento.");
            throw;
        }
    }
}