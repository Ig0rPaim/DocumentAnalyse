using Commons;
using Commons.Configuration;
using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Consumer;
using Google.GenAI;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCommonConfiguration(builder.Configuration);

builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IMinIoService, MinIoService>();
builder.Services.AddSingleton<IDocumentProcessor, GeminiDocumentProcessor>();
// builder.Services.AddSingleton<Client, Client>();


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
