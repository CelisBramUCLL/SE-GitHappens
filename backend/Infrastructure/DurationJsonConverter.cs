using System.Text.Json;
using System.Text.Json.Serialization;
using Dotnet_test.Domain;

namespace Dotnet_test.Infrastructure
{
    public class DurationJsonConverter : JsonConverter<Duration>
    {
        public override Duration Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var durationString = reader.GetString();
            if (string.IsNullOrEmpty(durationString))
            {
                return new Duration(0, 0);
            }
            return Duration.Parse(durationString);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Duration value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
