using System.Net;
using Microsoft.AspNetCore.Mvc;
using Producer.Factories.Interfaces;
using Producer.Services.Implementations;

namespace Producer.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    readonly IMinIoService _minioService;
    readonly  IkafkaService _kafkaService;
    readonly ILogger<DocumentController> _logger;
    
    [HttpGet(Name = "Post")]
    public async Task<IActionResult> Post(IFormFile file)
    {
        IFileService fileService = new FormFileService(file);
        
        if(!fileService.IsValid(out string[] errors))
            return Problem(string.Join(" ----- ", errors), statusCode: (int)HttpStatusCode.BadRequest, title: "Invalid file");

        string? objectName = null;
        try
        {
            objectName = await _minioService.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to save the file");
        }

        try
        {
            await _kafkaService.ProduceEvent(objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem("", "", (int)HttpStatusCode.InternalServerError, "Problems to generate event");
        }
        
        return Ok("O arquivo será analisádo");
    }
    
    
}
