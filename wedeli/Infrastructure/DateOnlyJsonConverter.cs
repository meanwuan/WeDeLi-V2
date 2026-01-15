using System.Text.Json;
using System.Text.Json.Serialization;

namespace wedeli.Infrastructure
{
    /// <summary>
    /// Custom JSON converter for formatting DateTime as dd/MM/yy in API responses
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateTime>
    {
        private const string DateFormat = "dd/MM/yy";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return default;

            // Try to parse dd/MM/yy format
            if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var date))
                return date;

            // Fallback to standard parsing
            if (DateTime.TryParse(dateString, out var fallbackDate))
                return fallbackDate;

            throw new JsonException($"Unable to parse '{dateString}' as DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat));
        }
    }

    /// <summary>
    /// Custom JSON converter for formatting nullable DateTime as dd/MM/yy in API responses
    /// </summary>
    public class NullableDateOnlyJsonConverter : JsonConverter<DateTime?>
    {
        private const string DateFormat = "dd/MM/yy";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
                return null;

            // Try to parse dd/MM/yy format
            if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var date))
                return date;

            // Fallback to standard parsing
            if (DateTime.TryParse(dateString, out var fallbackDate))
                return fallbackDate;

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(DateFormat));
            else
                writer.WriteNullValue();
        }
    }
}
