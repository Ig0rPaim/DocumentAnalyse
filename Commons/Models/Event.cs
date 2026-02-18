namespace Commons.Models;

public class Event
{
    public Guid Id { get; protected set; }

    public DateTime Date { get; protected set; }

    public string Name { get; protected set; }

    public string FullName
    {
        get
        {
            return $"{Id.ToString()}-{Name}";
        }
    }
    
    public Event(string name)
    {
        Id = Guid.NewGuid();
        Date = DateTime.UtcNow;
        Name = name;
    }
    public Event()
    {
        Id = Guid.NewGuid();
        Date = DateTime.UtcNow;
        Name = string.Empty;
    }
    
    
    
}
