using Commons;
using Consumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCommonConfiguration(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
