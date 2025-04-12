using System.Text.Json;
using System.Text.Json.Serialization;

namespace DH.SignalR;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private readonly String _format;

    public CustomDateTimeConverter(String format)
    {
        _format = format;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}