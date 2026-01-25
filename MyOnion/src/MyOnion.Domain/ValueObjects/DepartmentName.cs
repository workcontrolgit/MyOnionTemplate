using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyOnion.Domain.ValueObjects
{
    [JsonConverter(typeof(DepartmentNameJsonConverter))]
    public sealed class DepartmentName : IEquatable<DepartmentName>
    {
        public string Value { get; }

        public DepartmentName(string value)
        {
            Value = Normalize(value);
            if (Value.Length == 0)
            {
                throw new ArgumentException("Department name is required.", nameof(value));
            }

            if (Value.Length > 250)
            {
                throw new ArgumentException("Department name must not exceed 250 characters.", nameof(value));
            }
        }

        public static string Normalize(string value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        public bool Equals(DepartmentName other)
        {
            if (other is null)
            {
                return false;
            }

            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) => obj is DepartmentName other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;
    }

    internal sealed class DepartmentNameJsonConverter : JsonConverter<DepartmentName>
    {
        public override DepartmentName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return new DepartmentName(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DepartmentName value, JsonSerializerOptions options)
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
