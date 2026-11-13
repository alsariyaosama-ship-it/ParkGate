using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Sync;

public sealed class CloudPushService : ICloudPushService
{
    /// <summary>Keys that must never reach the cloud (Q-R2, ADR-021). Last line of defence behind the extractor.</summary>
    private static readonly string[] ForbiddenKeys = ["Phone", "NationalId"];

    private readonly HttpClient _httpClient;
    private readonly CloudApiOptions _options;
    private readonly ILogger<CloudPushService> _logger;

    public CloudPushService(
        HttpClient httpClient,
        IOptions<CloudApiOptions> options,
        ILogger<CloudPushService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // A malformed URL is reported by IsBaseUrlAllowed() at push time, not by throwing here.
        if (_httpClient.BaseAddress == null
            && Uri.TryCreate(_options.BaseUrl.EndsWith('/') ? _options.BaseUrl : _options.BaseUrl + "/", UriKind.Absolute, out var baseAddress))
        {
            _httpClient.BaseAddress = baseAddress;
        }

        if (!string.IsNullOrEmpty(_options.ApiKey) && !_httpClient.DefaultRequestHeaders.Contains("X-Api-Key"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("Accept"))
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        if (_httpClient.Timeout == TimeSpan.FromSeconds(100))
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(20);
        }
    }

    public async Task<PushBatchResult> PushBatchAsync(IReadOnlyList<SyncOutbox> batch, CancellationToken cancellationToken = default)
    {
        if (batch.Count == 0)
        {
            return PushBatchResult.Success();
        }

        if (!IsBaseUrlAllowed())
        {
            _logger.LogError("CloudApi:BaseUrl must be an https:// address. Nothing was pushed.");
            return PushBatchResult.Failure(PushFailureKind.Misconfigured, "CloudApi:BaseUrl must be an https:// address.");
        }

        var forbidden = batch.FirstOrDefault(r => ContainsForbiddenKey(r.PayloadJson));
        if (forbidden is not null)
        {
            // Log the entity only: the payload itself holds the personal data.
            _logger.LogError(
                "Outbox record {EntityType} {EntityId} carries a field that must not leave the park. Nothing was pushed.",
                forbidden.EntityType,
                forbidden.EntityId);
            return PushBatchResult.Failure(
                PushFailureKind.PayloadRejected,
                $"Record {forbidden.EntityType} {forbidden.EntityId} carries a field that must not leave the park.");
        }

        try
        {
            var requestPayload = new
            {
                records = batch.Select(r => new
                {
                    id = r.Id,
                    entityType = r.EntityType,
                    entityId = r.EntityId,
                    operationType = r.OperationType,
                    payloadJson = r.PayloadJson,
                    schemaVersion = r.SchemaVersion,
                    createdAtUtc = r.CreatedAtUtc
                })
            };

            var response = await _httpClient.PostAsJsonAsync("api/sync/push", requestPayload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully pushed batch of {Count} records to cloud.", batch.Count);
                return PushBatchResult.Success();
            }

            return await MapFailureAsync(response, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout while attempting to push sync batch to cloud API.");
            return PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, $"Request timed out: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP/Network error while attempting to push sync batch to cloud API.");
            return PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while attempting to push sync batch to cloud API.");
            return PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<PushBatchResult> SendHeartbeatAsync(int pendingCount, int failedCount, CancellationToken cancellationToken = default)
    {
        if (!IsBaseUrlAllowed())
        {
            _logger.LogError("CloudApi:BaseUrl must be an https:// address. No heartbeat was sent.");
            return PushBatchResult.Failure(PushFailureKind.Misconfigured, "CloudApi:BaseUrl must be an https:// address.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/sync/heartbeat", new { pendingCount, failedCount }, cancellationToken);
            return response.IsSuccessStatusCode
                ? PushBatchResult.Success()
                : await MapFailureAsync(response, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            _logger.LogWarning(ex, "Could not reach the cloud API to send a heartbeat.");
            return PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, $"Network error: {ex.Message}");
        }
    }

    private async Task<PushBatchResult> MapFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
            "Failed to push records to cloud. StatusCode: {StatusCode}, Details: {Details}",
            response.StatusCode,
            errorContent);

        var statusCode = response.StatusCode;
        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return PushBatchResult.Failure(PushFailureKind.Unauthorized, $"Status {(int)statusCode}: {errorContent}");
        }

        if (statusCode is HttpStatusCode.BadRequest || (int)statusCode == 422)
        {
            return PushBatchResult.Failure(PushFailureKind.PayloadRejected, $"Status {(int)statusCode}: {errorContent}");
        }

        if ((int)statusCode >= 500 && (int)statusCode <= 599)
        {
            return PushBatchResult.Failure(PushFailureKind.ServerError, $"Status {(int)statusCode}: {errorContent}");
        }

        return PushBatchResult.Failure(PushFailureKind.PayloadRejected, $"Status {(int)statusCode}: {errorContent}");
    }

    private bool IsBaseUrlAllowed()
    {
        if (!Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttps
            || (_options.AllowInsecureHttp && uri.Scheme == Uri.UriSchemeHttp);
    }

    /// <summary>Fails closed: a payload that cannot be parsed cannot be inspected, so it is treated as unsafe.</summary>
    private static bool ContainsForbiddenKey(string payloadJson)
    {
        try
        {
            return ContainsForbiddenKey(JsonNode.Parse(payloadJson));
        }
        catch (JsonException)
        {
            return true;
        }
    }

    private static bool ContainsForbiddenKey(JsonNode? node) => node switch
    {
        JsonObject obj => obj.Any(p =>
            ForbiddenKeys.Contains(p.Key, StringComparer.OrdinalIgnoreCase) || ContainsForbiddenKey(p.Value)),
        JsonArray array => array.Any(ContainsForbiddenKey),
        _ => false
    };
}
