using Commons.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Commons;

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
        
        // Registro dos Singletons usando IOptionsMonitor para obter sempre o valor atualizado
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptionsMonitor<KafkaSettings>>().CurrentValue);
        
        services.AddSingleton(sp => 
            sp.GetRequiredService<IOptionsMonitor<MinioSettings>>().CurrentValue);
        
        return services;
    }
}
