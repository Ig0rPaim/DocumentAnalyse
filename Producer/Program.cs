using Commons;
using Commons.Configuration;
using Commons.Models;
using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Google.GenAI;
using Producer.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMinIoService, MinIoService>();
builder.Services.AddSingleton<IProcessor, GeminiProcessor>();

builder.Services.AddCommonConfiguration(builder.Configuration);



// builder.Services.AddScoped<IMinIoService, MinIoService>();
// builder.Services.AddScoped<IDocumentProcessor, GeminiDocumentProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
