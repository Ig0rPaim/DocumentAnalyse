namespace Commons.Services.Interfaces;

public interface ISerializatorService
{
    public TResult DeserializeKeyType<TResult>(object serializedObject);
    public TResult DeserializeValueType<TResult>(object serializedObject);
    public object SerializeKeyType(object objectToSerialize);    
    public object SerializeValueType(object objectToSerialize);    
}
public interface ISerializatorService<TKey,TValue> : ISerializatorService 
{
    public TResult DeserializeKeyType<TResult>(TKey serializedObject);
    public TResult DeserializeValueType<TResult>(TValue serializedObject);
    public TKey SerializeKeyType(object objectToSerialize);    
    public TValue SerializeValueType(object objectToSerialize);
}

