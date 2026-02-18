using Commons.Models;

namespace Commons.Services.Interfaces;

public interface IEventService
{
    // public Event Event { get; set; }
    public  bool KeyIsValid(object? key);
    public bool ValueIsValid(object? value);
}
public interface IEventService<TKey, TValue> : IEventService
{
    // public Event Event { get; private set; }
    // public TKey Key { get; private set; }
    // public TValue Value { get; private set; }

    // protected EventWrapperGeneric(Event @event)
    // {
    //     Event = @event ?? throw new ArgumentNullException(nameof(@event));
    // }
    public  bool KeyIsValid(TKey? key);
    public bool ValueIsValid(TValue? value);

}