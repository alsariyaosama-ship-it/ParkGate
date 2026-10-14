using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Sync;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Sync;

public sealed class CloudPushServiceTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_statusCode == HttpStatusCode.OK ? "{\"status\":\"ok\"}" : "Internal Error")
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task PushBatchAsync_EmptyBatch_ReturnsTrueWithoutHttpRequest()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var result = await service.PushBatchAsync(Array.Empty<SyncOutbox>());

        result.ShouldBeTrue();
        handler.LastRequest.ShouldBeNull();
    }

    [Fact]
    public async Task PushBatchAsync_SuccessResponse_ReturnsTrueAndSetsHeaders()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "SECRET-KEY" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.ShouldBeTrue();
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest.Headers.GetValues("X-Api-Key").First().ShouldBe("SECRET-KEY");
        handler.LastRequest.RequestUri!.ToString().ShouldBe("https://api.test.ly/api/sync/push");
    }

    [Fact]
    public async Task PushBatchAsync_ErrorResponse_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.ShouldBeFalse();
    }
}
