using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Common.Kafka;

public class KafkaJsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
    }
}

public class KafkaJsonDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            throw new ArgumentNullException(nameof(data));
        }
        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<T>(json)!;
    }
}
