using Commons.Services.Interfaces;
using Newtonsoft.Json;

namespace Commons.Services.Implementations;

public class SerializatorStringService : ISerializatorService<string, string>
{
    #region string service
    public TResult Deserialize<TResult>(string serializedObject)
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        return JsonConvert.DeserializeObject<TResult>(serializedObject, settings);
    }

    public string Serialize(object objectToSerialize)
    {
        return JsonConvert.SerializeObject(objectToSerialize);
    }
    #endregion
    
    #region interface with generic
    public string SerializeKeyType(object objectToSerialize)
    {
        return Serialize(objectToSerialize);
    }
    
    public string SerializeValueType(object objectToSerialize)
    {
        return Serialize(objectToSerialize);
    }
    public TResult DeserializeKeyType<TResult>(string serializedObject)
    {
        return Deserialize<TResult>(serializedObject);
    }

    public TResult DeserializeValueType<TResult>(string serializedObject)
    {
        return Deserialize<TResult>(serializedObject);
    }
    #endregion

    #region interface without generic

        public TResult DeserializeKeyType<TResult>(object serializedObject)
        {
            return DeserializeKeyType<TResult>(serializedObject as string);
        }
        
        public TResult DeserializeValueType<TResult>(object serializedObject)
        {
            return DeserializeValueType<TResult>(serializedObject as string);
        }
        
        object ISerializatorService.SerializeKeyType(object objectToSerialize)
        {
            return SerializeKeyType(objectToSerialize as string);
        }
        
        object ISerializatorService.SerializeValueType(object objectToSerialize)
        {
            return SerializeValueType(objectToSerialize as string);
        }
    #endregion
}