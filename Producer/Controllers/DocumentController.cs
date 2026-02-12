using System.Net;
using Commons.Configuration;
using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    readonly IMinIoService _minioService;
    readonly  IKafkaProducerService _kafkaProducerService;
    readonly ILogger<DocumentController> _logger;
    readonly KafkaSettings _kafkaSettings;

    public DocumentController(IMinIoService minioService, IKafkaProducerService kafkaProducerService,
        ILogger<DocumentController> logger, KafkaSettings kafkaSettings)
    {
        _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
        _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
    }

    [HttpPost(Name = "PostDocumentToAnalyse")]
    public async Task<IActionResult> Post(IFormFile file)
    {
        IFileService fileService = new FormFileService(file);
        
        if(!fileService.IsValid(out string[] errors))
            return Problem(string.Join(" ----- ", errors), statusCode: (int)HttpStatusCode.BadRequest, title: "Invalid file");

        string? objectName = null;
        try
        {
            objectName = await _minioService.Save(fileService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to save the file");
        }

        try
        {
            await _kafkaProducerService.ProduceEvent(_kafkaSettings.TopicName, objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to generate event");
        }
        
        return Ok("O arquivo será analisádo");
    }
    
    
}
