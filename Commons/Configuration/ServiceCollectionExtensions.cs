using Commons.Models;
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
    public static Dictionary<Type[], Type> SerializerMappings = new(new TypeArrayComparer())
    {
        { [typeof(string), typeof(string)], typeof(SerializatorStringService) },
        // { [typeof(byte[])], typeof(SerializatorBinaryService) }
    };

    public static Dictionary<Type[], Type> EventServiceMappings = new(new TypeArrayComparer())
    {
        { [typeof(string), typeof(string)], typeof(EventStringService) }
    };
    
    public static IServiceCollection AddCommonConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection kafkaSettingsSection = configuration.GetSection("KafkaSettings");

        #region Get and monitore appsettings

        // Configuração do KafkaSettings com suporte a recarregamento (IOptionsMonitor)
        services.Configure<KafkaSettings>(
            kafkaSettingsSection);

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

        #endregion

        #region Add internal services
        
        var types = GetKafkaKeyValueTypes(kafkaSettingsSection);
        var typeArgs = new Type[] { types.keyType, types.valueType };
            
        AddDynamicImplementedServices(services, typeof(ISerializatorService<,>), SerializerMappings, typeArgs);
        
        AddDynamicImplementedServices(services, typeof(IEventService<,>), EventServiceMappings, typeArgs);
        
        AddDynamicService(services, typeof(IKafkaProducerService<,>), typeof(KafkaProducerService<,>), typeArgs);

        AddDynamicService(services, typeof(IKafkaConsumerService<,>), typeof(KafkaConsumerService<,>), typeArgs);

        
        
        #endregion

        #region Add external services
        services.AddSingleton(typeof(IProducer<,>).MakeGenericType(typeArgs), sp => 
        {
            var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            var config = new ProducerConfig { BootstrapServers = kafkaSettings.BootstrapServers };
    
            return BuildKafkaComponent(sp, typeof(ProducerBuilder<,>), typeArgs, config);
        });

        services.AddSingleton(typeof(IConsumer<,>).MakeGenericType(typeArgs), sp => 
        {
            var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            var config = new ConsumerConfig 
            { 
                BootstrapServers = kafkaSettings.BootstrapServers,
                GroupId = kafkaSettings.GroupId,
                AutoOffsetReset = (AutoOffsetReset)kafkaSettings.AutoOffsetReset,
                EnableAutoCommit = kafkaSettings.EnableAutoCommit
            };

            return BuildKafkaComponent(sp, typeof(ConsumerBuilder<,>), typeArgs, config);
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

        #endregion

        return services;
    }

    private static (Type keyType, Type valueType) GetKafkaKeyValueTypes(IConfigurationSection section)
    {
        var settings = section.Get<KafkaSettings>() ??
                       throw new InvalidCastException("KafkaSettings bad formated in appsettings");
        Type keyType = Type.GetType(settings.KeyType) ?? typeof(string);
        Type valueType = Type.GetType(settings.ValueType) ?? typeof(string);
        return (keyType, valueType);
    }
    
    private static void AddDynamicService(
        IServiceCollection services, 
        Type serviceInterface,
        Type implementationType,
        Type[] genericArgs) 
    {
        Type closedInterface = serviceInterface.MakeGenericType(genericArgs);
        Type closedImplementation = implementationType.MakeGenericType(genericArgs);

        services.AddSingleton(closedInterface, closedImplementation);
    }
    
    private static object BuildKafkaComponent(IServiceProvider sp, Type builderGenericType, Type[] genericArgs, object config)
    {
        Type builderType = builderGenericType.MakeGenericType(genericArgs);

        object builderInstance = Activator.CreateInstance(builderType, config) 
                                 ?? throw new InvalidOperationException($"Não foi possível instanciar {builderType.Name}");

        var buildMethod = builderType.GetMethod("Build") 
                          ?? throw new InvalidOperationException("Método Build não encontrado.");

        return buildMethod.Invoke(builderInstance, null)!;
    }

    private static void AddDynamicImplementedServices(IServiceCollection services, Type interfaceType, Dictionary<Type[],Type> mapping, params Type[] genericTypes)
    {
        // 1. Define a interface que estamos tentando satisfazer: ISerializatorService<TValue>
        Type serviceInterface = interfaceType.MakeGenericType(genericTypes);

        // 2. Busca a implementação no nosso mapa
        if (!mapping.TryGetValue(genericTypes, out Type implementationType))
        {
            throw new NotSupportedException($"Nenhum serializador registrado para o tipo(s): {string.Join(", ", genericTypes.Select(g => g.Name))}");
        }

        // 3. Verifica se a implementação é genérica aberta (ex: SerializatorJson<>) 
        // ou fechada (ex: SerializatorStringService)
        Type closedImplementation = implementationType.IsGenericTypeDefinition
            ? implementationType.MakeGenericType(genericTypes)
            : implementationType;

        services.AddSingleton(serviceInterface, closedImplementation);
    }
}

public class TypeArrayComparer : IEqualityComparer<Type[]>
{
    public bool Equals(Type[]? x, Type[]? y)
    {
        if (x == null || y == null) return x == y;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(Type[] obj)
    {
        if (obj == null) return 0;
        int hash = 17;
        foreach (var t in obj)
        {
            hash = hash * 31 + (t?.GetHashCode() ?? 0);
        }
        return hash;
    }
}