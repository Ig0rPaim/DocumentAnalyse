using Producer.Factories.Interfaces;

namespace Producer.Services.Implementations;

public class FormFileService : IFileService
{
    private readonly IFormFile _file;

    public FormFileService(IFormFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    public Stream Stream()
    {
        return _file.OpenReadStream();
    }

    public string Name()
    {
        return _file.FileName;
    }

    public long Length()
    {
        return _file.Length;
    }

    public string ContentType()
    {
        return _file.ContentType;
    }

    public bool IsValid(out string[] errors)
    {
        errors = new string[3];
        bool valid = true;
        if (_file.Length <= 0)
        {
           valid = false;
           errors[0] = "File is empty";
        }

        if (_file.ContentType != "application/pdf")
        {
            valid = false;
            errors[1] = "Type no support"; 
        }

        if (Path.GetExtension(_file.FileName) != "pdf")
        {
            valid = false;
            errors[2] = "Extension no support";
        }
        errors = errors.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        
        return valid;
    }
}