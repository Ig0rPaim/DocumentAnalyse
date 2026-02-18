namespace Commons.Services.Interfaces;

public interface IMinIoService
{
    public Task<string> Save(IFileService fileService, string objectName = null);
    public Task<(Stream file, string fileName)> Get(string objectName);
}