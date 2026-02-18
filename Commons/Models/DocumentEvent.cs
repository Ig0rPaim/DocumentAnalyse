using System.Runtime.InteropServices.JavaScript;

namespace Commons.Models;

public class DocumentEvent : Event
{
    public string Bucket { get; set; }

    public DocumentEvent(string name, string bucket) : base(name)
    {
        Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
    }
}