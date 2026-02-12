using Commons.Configuration;
using Minio;
using Minio.DataModel.Args;
using Producer.Factories.Interfaces;

namespace Producer.Services.Implementations;

public class MinIoService : IMinIoService
{
    readonly IFileService _fileService;
    readonly MinioSettings _minioSettings;
    readonly IMinioClient _minioClient;
    
    public async Task<string> Save()
    {
        var objectName = $"{Guid.NewGuid()}-{_fileService.Name()}";
        
        using Stream stream = _fileService.Stream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_minioSettings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(_fileService.Length())
            .WithContentType(_fileService.ContentType());
        
        await _minioClient.PutObjectAsync(putObjectArgs);
        
        return  objectName;
    }
}