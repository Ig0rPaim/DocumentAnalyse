using System.Text.Json;
using Commons.Configuration;
using Confluent.Kafka;
using Producer.Factories.Interfaces;

namespace Producer.Services.Implementations;

public class KafkaService : IkafkaService
{
    readonly KafkaSettings _kafkaSettings;
    readonly MinioSettings _minioSettings;
    readonly IProducer<string, string> _kafkaProducer;
    
    public async Task ProduceEvent(string? objectName)
    {
        if(string.IsNullOrEmpty(objectName))
            throw  new ArgumentNullException(nameof(objectName));
        
        var mensagemJson = JsonSerializer.Serialize(new {
            Id = Guid.NewGuid(),
            NomeArquivo = objectName,
            Bucket = _minioSettings.BucketName,
            DataUpload = DateTime.UtcNow
        });

        var kafkaMessage = new Message<string, string> { 
            Key = objectName, 
            Value = mensagemJson 
        };

        await _kafkaProducer.ProduceAsync("analise-documentos", kafkaMessage);
    }
}