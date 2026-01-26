using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyOnion.Domain.ValueObjects
{
    [JsonConverter(typeof(PositionTitleJsonConverter))]
    public sealed class PositionTitle : IEquatable<PositionTitle>
    {
        public string Value { get; }

        private PositionTitle()
        {
        }

        public PositionTitle(string value)
        {
            Value = Normalize(value);
            if (Value.Length == 0)
            {
                throw new ArgumentException("Position title is required.", nameof(value));
            }

            if (Value.Length > 250)
            {
                throw new ArgumentException("Position title must not exceed 250 characters.", nameof(value));
            }
        }

        public static string Normalize(string value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        public bool Equals(PositionTitle other)
        {
            if (other is null)
            {
                return false;
            }

            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => obj is PositionTitle other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;
    }

    internal sealed class PositionTitleJsonConverter : JsonConverter<PositionTitle>
    {
        public override PositionTitle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return new PositionTitle(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, PositionTitle value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value);
        }
    }
}
