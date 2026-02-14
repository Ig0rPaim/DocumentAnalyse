using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Google.GenAI;
using Minio;

namespace Commons.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        
        // Configuração do KafkaSettings com suporte a recarregamento (IOptionsMonitor)
        services.Configure<KafkaSettings>(
            configuration.GetSection("KafkaSettings"));
        
        // Configuração do MinioSettings com suporte a recarregamento (IOptionsMonitor)
        services.Configure<MinioSettings>(
            configuration.GetSection("MinioSettings"));
        
        // Configuração do AiSettings com suporte a recarregamento (IOptionsMonitor)
        services.Configure<AiSettings>(
            configuration.GetSection("AiSettings"));
        
        // Registro dos Singletons usando IOptionsMonitor para obter sempre o valor atualizado
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptionsMonitor<KafkaSettings>>().CurrentValue);
        
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptionsMonitor<MinioSettings>>().CurrentValue);
        
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptionsMonitor<AiSettings>>().CurrentValue);


        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            return new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
        });

        // Registro do Consumer usando as configurações do KafkaSettings
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = config.GroupId,
                AutoOffsetReset = (AutoOffsetReset)config.AutoOffsetReset, // Aqui ele usa o valor lido do JSON
                EnableAutoCommit = config.EnableAutoCommit
            };

            return new ConsumerBuilder<string, string>(consumerConfig).Build();
        });

        services.AddSingleton<IMinioClient>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
            return new MinioClient()
                .WithEndpoint(config.Endpoint)
                .WithCredentials(config.AccessKey, config.SecretKey)
                .Build();
        });

        services.AddSingleton<Client>(sp =>
        {
            var settings = sp.GetRequiredService<AiSettings>();
            return new Client(apiKey: settings.ApiKey);
        });

        return services;
    }
}
