namespace Producer.Factories.Interfaces;

public interface IkafkaService
{
    public Task ProduceEvent(string? objectName);
}