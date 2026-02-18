using Commons.Models;

namespace Commons.Services.Interfaces;

public interface IProcessor
{
    public Task<Event> Process(Event @event);
}