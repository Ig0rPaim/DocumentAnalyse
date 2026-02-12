namespace Commons.Services.Interfaces;

public interface IDocumentProcessor
{
    public Task<string> Process(Stream file);
}