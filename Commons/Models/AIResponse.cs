using System.Text.Json;

namespace Commons.Models;

public class AIResponse
{
    double _duration = 0;
    public JsonElement Fields { get; set; } = JsonDocument.Parse("{}").RootElement;
        
    public string RawResponse { get; set; } = string.Empty;
    public double Duraration 
    { 
        get { return Math.Round(_duration, 2); }
        set { _duration = value; }
    }
    public bool Success { get; set; } = false;
}