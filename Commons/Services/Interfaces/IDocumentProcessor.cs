using Commons.Models;

namespace Commons.Services.Interfaces;

public interface IDocumentProcessor
{
    public Task<AIResponse> Process(Stream file, string fileName);
}