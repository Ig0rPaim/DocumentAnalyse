using System.Text.Json;
using Commons.Configuration;
using Commons.Models;
using Commons.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.VisualBasic.CompilerServices;

namespace Commons.Services.Implementations;

public class KafkaProducerService : IKafkaProducerService
{
    readonly KafkaSettings _kafkaSettings;
    readonly MinioSettings _minioSettings;
    readonly IProducer<string, string> _kafkaProducer;

    public KafkaProducerService(KafkaSettings kafkaSettings, MinioSettings minioSettings,
        IProducer<string, string> kafkaProducer)
    {
        _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
        _minioSettings = minioSettings ?? throw new ArgumentNullException(nameof(minioSettings));
        _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
    }

    public async Task ProduceEvent(string topic, string? objectName)
    {
        if(string.IsNullOrEmpty(topic))
            throw  new ArgumentNullException(nameof(topic));
        
        if(string.IsNullOrEmpty(objectName))
            throw  new ArgumentNullException(nameof(objectName));

        DocumentEvent documentEvent =
            new DocumentEvent(Guid.NewGuid(), objectName, _minioSettings.BucketName, DateTime.UtcNow);
        var mensagemJson = JsonSerializer.Serialize(documentEvent);

        var kafkaMessage = new Message<string, string> { 
            Key = objectName, 
            Value = mensagemJson 
        };

        await _kafkaProducer.ProduceAsync(topic, kafkaMessage);
    }

    public Task ConsumeEvent(string topic)
    {
        throw new NotImplementedException();
    }
}