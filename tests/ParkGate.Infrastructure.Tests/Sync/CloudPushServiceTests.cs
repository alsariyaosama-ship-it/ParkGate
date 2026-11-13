using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Sync;
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
        public string? LastBody { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null ? null : request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_statusCode == HttpStatusCode.OK ? "{\"status\":\"ok\"}" : "Error Details")
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task PushBatchAsync_EmptyBatch_ReturnsSuccessWithoutHttpRequest()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var result = await service.PushBatchAsync(Array.Empty<SyncOutbox>());

        result.IsSuccess.ShouldBeTrue();
        result.FailureKind.ShouldBe(PushFailureKind.None);
        handler.LastRequest.ShouldBeNull();
    }

    [Fact]
    public async Task PushBatchAsync_SuccessResponse_ReturnsSuccessAndSetsHeaders()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "SECRET-KEY" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeTrue();
        result.FailureKind.ShouldBe(PushFailureKind.None);
        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest.Headers.GetValues("X-Api-Key").First().ShouldBe("SECRET-KEY");
        handler.LastRequest.RequestUri!.ToString().ShouldBe("https://api.test.ly/api/sync/push");
    }

    [Fact]
    public async Task PushBatchAsync_ServerErrorResponse_ReturnsServerErrorKind()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(PushFailureKind.ServerError);
    }

    [Fact]
    public async Task PushBatchAsync_UnauthorizedResponse_ReturnsUnauthorizedKind()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(PushFailureKind.Unauthorized);
    }

    [Fact]
    public async Task PushBatchAsync_UnprocessableEntity_ReturnsPayloadRejectedKind()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.UnprocessableEntity);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var service = new CloudPushService(httpClient, options, NullLogger<CloudPushService>.Instance);

        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(PushFailureKind.PayloadRejected);
    }

    private static CloudPushService CreateService(FakeHttpMessageHandler handler, CloudApiOptions options) =>
        new(new HttpClient(handler), Options.Create(options), NullLogger<CloudPushService>.Instance);

    [Theory]
    [InlineData("{\"Id\":\"A\",\"Phone\":\"0912345678\"}")]
    [InlineData("{\"Id\":\"A\",\"NationalId\":\"119901234567\"}")]
    [InlineData("{\"Id\":\"A\",\"phone\":\"0912345678\"}")]
    [InlineData("{\"Id\":\"A\",\"nationalid\":\"x\"}")]
    [InlineData("{\"Id\":\"A\",\"Nested\":{\"Phone\":\"x\"}}")]
    [InlineData("{\"Id\":\"A\",\"Items\":[{\"NationalId\":\"x\"}]}")]
    public async Task PushBatchAsync_PayloadWithSensitiveKey_MakesNoHttpCallAndIsRejected(string payload)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        var clean = SyncOutbox.Create("Organization", "ORG-1", "Insert", "{\"Id\":1}", DateTime.UtcNow);
        var dirty = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", payload, DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { clean, dirty });

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(PushFailureKind.PayloadRejected);
        handler.LastRequest.ShouldBeNull();
        result.ErrorMessage!.ShouldNotContain("0912345678");
        result.ErrorMessage!.ShouldNotContain("119901234567");
    }

    [Fact]
    public async Task PushBatchAsync_PayloadWithoutSensitiveKeys_IsPushed()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });
        // "PhoneticName" and a value that merely mentions "Phone" are not the Phone key.
        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"PhoneticName\":\"x\",\"Notes\":\"Phone call\"}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeTrue();
        handler.LastRequest.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("http://api.test.ly")]
    [InlineData("ftp://api.test.ly")]
    [InlineData("api.test.ly")]
    [InlineData("")]
    public async Task PushBatchAsync_BaseUrlThatIsNotHttps_MakesNoHttpCallAndReportsMisconfiguration(string baseUrl)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = baseUrl, ApiKey = "KEY123" });
        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(PushFailureKind.Misconfigured);
        handler.LastRequest.ShouldBeNull();
    }

    [Fact]
    public async Task PushBatchAsync_HttpBaseUrlWithAllowInsecureHttp_IsPushed()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "http://localhost:8000", ApiKey = "KEY123", AllowInsecureHttp = true });
        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{}", DateTime.UtcNow);

        var result = await service.PushBatchAsync(new[] { record });

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task SendHeartbeatAsync_PostsCountsToHeartbeatEndpoint()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });

        var result = await service.SendHeartbeatAsync(pendingCount: 7, failedCount: 2);

        result.IsSuccess.ShouldBeTrue();
        handler.LastRequest!.RequestUri!.AbsolutePath.ShouldEndWith("api/sync/heartbeat");
        handler.LastRequest.Method.ShouldBe(HttpMethod.Post);
        handler.LastBody.ShouldNotBeNull();
        using var doc = System.Text.Json.JsonDocument.Parse(handler.LastBody);
        doc.RootElement.GetProperty("pendingCount").GetInt32().ShouldBe(7);
        doc.RootElement.GetProperty("failedCount").GetInt32().ShouldBe(2);
    }

    [Fact]
    public async Task SendHeartbeatAsync_BaseUrlThatIsNotHttps_MakesNoHttpCall()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "http://api.test.ly", ApiKey = "KEY123" });

        var result = await service.SendHeartbeatAsync(0, 0);

        result.FailureKind.ShouldBe(PushFailureKind.Misconfigured);
        handler.LastRequest.ShouldBeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, PushFailureKind.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError, PushFailureKind.ServerError)]
    [InlineData(HttpStatusCode.UnprocessableEntity, PushFailureKind.PayloadRejected)]
    public async Task SendHeartbeatAsync_ErrorResponse_MapsToFailureKind(HttpStatusCode status, PushFailureKind expected)
    {
        var handler = new FakeHttpMessageHandler(status);
        var service = CreateService(handler, new CloudApiOptions { BaseUrl = "https://api.test.ly", ApiKey = "KEY123" });

        var result = await service.SendHeartbeatAsync(0, 0);

        result.IsSuccess.ShouldBeFalse();
        result.FailureKind.ShouldBe(expected);
    }
}
