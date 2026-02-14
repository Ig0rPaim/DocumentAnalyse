using Commons.Configuration;
using Commons.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Commons.Services.Implementations;

public class MinIoService : IMinIoService
{
    readonly MinioSettings _minioSettings;
    readonly IMinioClient _minioClient;
    readonly ILogger<MinIoService> _logger;


    public MinIoService(MinioSettings minioSettings, IMinioClient minioClient)
    {
        _minioSettings = minioSettings ?? throw new ArgumentNullException(nameof(minioSettings));
        _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
    }

    public async Task<string> Save(IFileService fileService)
    {
        var objectName = $"{Guid.NewGuid()}-{fileService.Name()}";
        
        using Stream stream = fileService.Stream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_minioSettings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(fileService.Length())
            .WithContentType(fileService.ContentType());
        
        await _minioClient.PutObjectAsync(putObjectArgs);
        
        return  objectName;
    }

    public async Task<(Stream file, string fileName)> Get(string objectName)
    {
        try
        {
            var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_minioSettings.BucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(memoryStream);
                });

            await _minioClient.GetObjectAsync(args);
            
            memoryStream.Position = 0;
            
            return  (memoryStream, objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao baixar arquivo {objectName}: {ex.Message}");
            throw; 
        }
        
    }
}