using System.Net;
using Commons.Configuration;
using Commons.Models;
using Commons.Services.Implementations;
using Commons.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Producer.Services.Implementations;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController(
    IMinIoService minioService,
    IKafkaProducerService kafkaProducerService,
    ILogger<DocumentController> logger,
    KafkaSettings kafkaSettings,
    MinioSettings minioSettings)
    : ControllerBase
{
    readonly IMinIoService _minioService = minioService ?? throw new ArgumentNullException(nameof(minioService));
    readonly  IKafkaProducerService _kafkaProducerService = kafkaProducerService ?? throw new ArgumentNullException(nameof(kafkaProducerService));
    readonly ILogger<DocumentController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    readonly KafkaSettings _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException(nameof(kafkaSettings));
    readonly MinioSettings _minioSettings = minioSettings ?? throw new ArgumentNullException(nameof(minioSettings));

    [HttpPost(Name = "PostDocumentToAnalyse")]
    public async Task<IActionResult> Post(IFormFile file)
    {
        IFileService fileService = new FormFileService(file);
        
        if(!fileService.IsValid(out string[] errors))
            return Problem(string.Join(" ----- ", errors), statusCode: (int)HttpStatusCode.BadRequest, title: "Invalid file");

        Event documentEvent = new DocumentEvent(fileService.Name(), _minioSettings.BucketName);
        try
        {
            _=await _minioService.Save(fileService, documentEvent.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to save the file");
        }

        try
        {   
            await _kafkaProducerService.ProduceEvent(_kafkaSettings.PostFileTopicName, documentEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to generate event");
        }
        
        return Ok("O arquivo será analisádo");
    }
    
    
}
