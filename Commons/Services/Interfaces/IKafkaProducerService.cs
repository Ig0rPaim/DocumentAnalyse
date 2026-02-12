namespace Commons.Services.Interfaces;

public interface IKafkaProducerService
{
    public Task ProduceEvent(string topic, string? objectName);
}