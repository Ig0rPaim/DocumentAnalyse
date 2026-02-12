namespace Commons.Services.Interfaces;

public interface IMinIoService
{
    public Task<string> Save(IFileService fileService);
    public Task<Stream> Get(string objectName);
}