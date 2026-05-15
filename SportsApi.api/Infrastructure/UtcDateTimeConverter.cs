using System.Text.Json;
using System.Text.Json.Serialization;

namespace SportsApi.api.Infrastructure;

/// <summary>
/// Ensures every DateTime received from the client is treated as UTC.
/// This is required by Npgsql when writing to a PostgreSQL "timestamp with time zone" column.
/// The client may send plain date strings ("2026-05-16") or ISO-8601 with time
/// ("2026-05-16T15:00:00") — both are parsed and their Kind is forced to Utc before
/// being passed down to the domain / EF Core.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime());
    }
}

