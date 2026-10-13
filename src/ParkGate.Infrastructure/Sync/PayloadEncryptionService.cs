using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Security;

namespace ParkGate.Infrastructure.Sync;

public sealed class PayloadEncryptionService : IPayloadEncryptionService
{
    private readonly string? _aesKey;
    private readonly ILogger<PayloadEncryptionService> _logger;

    private static readonly Dictionary<string, HashSet<string>> SensitiveFields = new(StringComparer.Ordinal)
    {
        {
            nameof(Beneficiary), new HashSet<string>(StringComparer.Ordinal)
            {
                "NationalId",
                "Phone"
            }
        }
    };

    public PayloadEncryptionService(IOptions<CloudApiOptions> options, ILogger<PayloadEncryptionService> logger)
    {
        _aesKey = options.Value.AesKey;
        _logger = logger;
    }

    public PayloadEncryptionService(string? aesKey, ILogger<PayloadEncryptionService> logger)
    {
        _aesKey = aesKey;
        _logger = logger;
    }

    public string EncryptSensitiveFields(string entityType, string payloadJson)
    {
        if (string.IsNullOrEmpty(_aesKey) || string.IsNullOrEmpty(payloadJson))
        {
            return payloadJson;
        }

        if (!SensitiveFields.TryGetValue(entityType, out var fieldsToEncrypt))
        {
            return payloadJson;
        }

        try
        {
            var jsonNode = JsonNode.Parse(payloadJson);
            if (jsonNode is not JsonObject jsonObj)
            {
                return payloadJson;
            }

            bool modified = false;

            foreach (var field in fieldsToEncrypt)
            {
                if (jsonObj.ContainsKey(field) && jsonObj[field] != null)
                {
                    string? rawValue = jsonObj[field]?.ToString();
                    if (!string.IsNullOrEmpty(rawValue))
                    {
                        string encryptedValue = AesHelper.Encrypt(rawValue, _aesKey);
                        jsonObj[field] = encryptedValue;
                        modified = true;
                    }
                }
            }

            return modified ? jsonObj.ToJsonString() : payloadJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt sensitive fields for entity {EntityType}", entityType);
            throw;
        }
    }
}
