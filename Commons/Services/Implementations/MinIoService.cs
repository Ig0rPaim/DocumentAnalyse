using Commons.Configuration;
using Commons.Services.Interfaces;
using Minio;
using Minio.DataModel.Args;

namespace Commons.Services.Implementations;

public class MinIoService : IMinIoService
{
    readonly MinioSettings _minioSettings;
    readonly IMinioClient _minioClient;

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

    public Task<Stream> Get(string objectName)
    {
        throw new NotImplementedException();
    }
}