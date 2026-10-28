using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Sync;

public sealed class CloudPushService : ICloudPushService
{
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

        if (!string.IsNullOrEmpty(_options.BaseUrl) && _httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.EndsWith('/') ? _options.BaseUrl : _options.BaseUrl + "/");
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
}
