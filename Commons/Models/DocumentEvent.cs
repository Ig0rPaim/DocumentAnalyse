namespace Commons.Models;

public class DocumentEvent
{
    public Guid Id { get; set; }
    public string ObjectName { get; set; }
    public string Bucket { get; set; }
    public DateTime UploadDate { get; set; }

    public DocumentEvent(Guid id, string objectName, string bucket, DateTime uploadDate)
    {
        Id = id;
        ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
        UploadDate = uploadDate;
    }
}