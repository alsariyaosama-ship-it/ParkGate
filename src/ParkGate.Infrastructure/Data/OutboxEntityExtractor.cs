using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Extracts and serializes domain entity state into JSON outbox payloads for cloud sync.
/// </summary>
public static class OutboxEntityExtractor
{
    private static readonly HashSet<string> SyncableTypes = new(StringComparer.Ordinal)
    {
        nameof(Beneficiary),
        nameof(EntryAttempt),
        nameof(EntryAttemptCompanion),
        nameof(Organization),
        nameof(AgeCategory)
    };

    public static bool IsSyncable(string entityTypeName) => SyncableTypes.Contains(entityTypeName);

    public static string ExtractEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key == null)
        {
            return string.Empty;
        }

        var keyValues = key.Properties
            .Select(p => FormatValue(entry.Property(p.Name).CurrentValue)?.ToString() ?? string.Empty);

        return string.Join("_", keyValues);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public static string SerializePayload(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var prop in entry.Properties)
        {
            if (prop.Metadata.IsShadowProperty())
            {
                continue;
            }

            dict[prop.Metadata.Name] = FormatValue(prop.CurrentValue);
        }

        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    public static object? FormatValue(object? val)
    {
        return val switch
        {
            null => null,
            CardNumber cn => cn.Value,
            PhoneNumber pn => pn.Value,
            NationalId nid => nid.Value,
            DateOnly d => d.ToString("yyyy-MM-dd"),
            DateTime dt => dt.ToString("O"),
            byte[] bytes => Convert.ToBase64String(bytes),
            Enum e => Convert.ToInt32(e),
            _ => val
        };
    }
}
