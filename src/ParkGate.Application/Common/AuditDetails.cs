using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ParkGate.Application.Common;

/// <summary>
/// Builds the details JSON of an audit row (R3.4). Values are serialized, never interpolated, so a reason that
/// contains a quote or a backslash still yields valid JSON, and numbers do not depend on the current culture.
/// Arabic stays readable in the stored text instead of being escaped as \uXXXX.
/// </summary>
public static class AuditDetails
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
    };

    /// <summary>Serializes an anonymous object such as <c>new { Reason = request.Reason }</c>.</summary>
    public static string ToJson(object details) => JsonSerializer.Serialize(details, Options);
}
