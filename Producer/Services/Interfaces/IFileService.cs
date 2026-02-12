namespace Producer.Factories.Interfaces;

public interface IFileService
{
    public Stream Stream();
    public string Name();
    public long Length();
    public string ContentType();
    public bool IsValid(out string[] errors);
}