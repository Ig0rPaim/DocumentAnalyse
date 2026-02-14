using System.Text.Json;
using Commons.Configuration;
using Commons.Models;
using Commons.Services.Interfaces;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;

namespace Commons.Services.Implementations;

public class GeminiDocumentProcessor : IDocumentProcessor
{
    private readonly ILogger<GeminiDocumentProcessor> _logger;
    private readonly Client _googleClient;
    private readonly AiSettings _aiSettings;

    public GeminiDocumentProcessor(ILogger<GeminiDocumentProcessor> logger, Client googleClient, AiSettings aiSettings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _googleClient = googleClient ?? throw new ArgumentNullException(nameof(googleClient));
        _aiSettings = aiSettings ?? throw new ArgumentNullException(nameof(aiSettings));
    }

    public async Task<AIResponse> Process(Stream file, string fileName)
    {
        AIResponse aiResponse = new();
        DateTime inicio = DateTime.Now;
        
        #region send request
        Google.GenAI.Types.File uploadResponse;
        try
        {
            uploadResponse = await TryUpload(_googleClient, file, fileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error to load file to Gemini AI");
            return RetornoException(e, aiResponse); 
        }

        GenerateContentResponse response;
        try
        {
            response = await TryProcess(_googleClient, uploadResponse?.Uri!, _aiSettings.Model, _aiSettings.Prompt);
        }
        catch (Exception e)
        {
            return RetornoException(e, aiResponse);
        }
        #endregion
        
        string message = response?.Candidates?[0]?.Content?.Parts?[0]?.Text!;
        
        if (string.IsNullOrEmpty(message))
            return RetornoExceptionMsg("Resposta da API sem mensagem ou formato inesperado", aiResponse);
        
        aiResponse.RawResponse = message;
        
        try
        {
            aiResponse.Fields = GetJsonFromMessage(message);
        }
        catch (Exception e)
        {
            return RetornoException(e, aiResponse);
        }
        
        try
        {
            aiResponse.Duraration = GetRequestDuration(inicio);
            if (aiResponse.Duraration < 0)
                return RetornoExceptionMsg("Falha ao calcular duração da requisição", aiResponse);

        }
        catch (Exception e)
        {
            return RetornoException(e, aiResponse);

        }
        
        aiResponse.Success = true;
        return aiResponse;

    }

    async Task<Google.GenAI.Types.File> TryUpload(Client client, Stream stream, string fileName)
    {
        Google.GenAI.Types.File uploadResponse =
            await client.Files.UploadAsync(stream, stream.Length, "", "application/pdf");
        if (uploadResponse == null || string.IsNullOrEmpty(uploadResponse.Uri))
            throw new InvalidCastException("Upload falhou ou resposta sem URI");
        return uploadResponse;
    }

    async Task<GenerateContentResponse> TryProcess(Client client, string uriToFile, string modelo, string prompt)
    {
        return await client.Models.GenerateContentAsync(
            model: modelo,
            contents: new List<Content>
            {
                new Content
                {
                    Parts = new List<Part>
                    {
                        new Part { FileData = new FileData { FileUri = uriToFile, MimeType = "application/pdf" } },
                        new Part { Text = prompt }
                    }
                }
            }
        );
    }

    #region support methods

    AIResponse RetornoException(Exception e, AIResponse retorno)
    {
        retorno.Success = false;
        retorno.Fields = new JsonElement();
        retorno.RawResponse = e.Message;
        return retorno;
    }

    AIResponse RetornoExceptionMsg(string message, AIResponse retorno)
    {
        retorno.Success = false;
        retorno.Fields = new JsonElement();
        retorno.RawResponse = message;
        return retorno;
    }
    
    JsonElement GetJsonFromMessage(string message) 
    {
        try
        {
            if(!message.Contains("```json") || !message.Contains("```"))
                return JsonDocument.Parse("{}").RootElement;
            string json = message.Replace("```json", "").Replace("```", "").Trim();
            JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement;
        }
        catch(Exception e)
        {
            throw new InvalidCastException("Failed to parse JSON from API response", e);
        }
    }
    
    double GetRequestDuration(DateTime init)
    {
        string durationString = (DateTime.Now - init).TotalSeconds.ToString();
        if (double.TryParse(durationString, out double duration))
        {
            return duration;
        }
        return -1;
    }
    #endregion
}