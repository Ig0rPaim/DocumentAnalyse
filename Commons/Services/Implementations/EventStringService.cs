using Commons.Models;
using Commons.Services.Interfaces;
using Newtonsoft.Json;

namespace Commons.Services.Implementations;

public class EventStringService : IEventService<string, string>
{
    public bool KeyIsValid(object? key)
    {
        return KeyIsValid(key as string);
    }

    public bool ValueIsValid(object? value)
    {
        return ValueIsValid(value as string);
    }

    public bool KeyIsValid(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        return true;
    }

    public bool ValueIsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
        return true;
    }
}
