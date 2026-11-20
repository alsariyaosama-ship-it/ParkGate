using System.Runtime.Versioning;
using ParkGate.Application.Sync;
using ParkGate.Infrastructure.Sync;
using ParkGate.SyncAgent;

namespace ParkGate.Infrastructure.Tests.Sync;

/// <summary>R8.3: the Sync Agent's cloud API key is stored with DPAPI and set from the command line.</summary>
[SupportedOSPlatform("windows")]
public sealed class CloudApiKeyTests : IDisposable
{
    private readonly string _file = Path.Combine(Path.GetTempPath(), $"parkgate_test_cloudkey_{Guid.NewGuid():N}.dat");
    private readonly DpapiCloudApiKeyStore _store;

    public CloudApiKeyTests()
    {
        _store = new DpapiCloudApiKeyStore(_file);
    }

    public void Dispose()
    {
        if (File.Exists(_file))
        {
            File.Delete(_file);
        }
    }

    [Fact]
    public void Store_SaveAndGet_RoundTripsWithoutPlainTextOnDisk()
    {
        _store.SaveApiKey("  secret-key-123  ");

        _store.GetApiKey().ShouldBe("secret-key-123");
        System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(_file)).ShouldNotContain("secret-key-123");
    }

    [Fact]
    public void Store_NoFile_ReturnsNull() => _store.GetApiKey().ShouldBeNull();

    [Fact]
    public void Store_FileNotProtectedHere_ReturnsNull()
    {
        File.WriteAllText(_file, "not a DPAPI blob");

        _store.GetApiKey().ShouldBeNull();
    }

    [Fact]
    public void Store_Clear_RemovesTheKey()
    {
        _store.SaveApiKey("k");

        _store.Clear().ShouldBeTrue();
        _store.GetApiKey().ShouldBeNull();
        _store.Clear().ShouldBeFalse();
    }

    [Fact]
    public void PostConfigure_StoredKey_WinsOverThePlainJsonKey()
    {
        _store.SaveApiKey("from-dpapi");
        var options = new CloudApiOptions { ApiKey = "from-json" };

        new CloudApiKeyPostConfigure(_store).PostConfigure(null, options);

        options.ApiKey.ShouldBe("from-dpapi");
    }

    [Fact]
    public void PostConfigure_NoStoredKey_KeepsTheJsonKeyForUpgradedAgents()
    {
        var options = new CloudApiOptions { ApiKey = "from-json" };

        new CloudApiKeyPostConfigure(_store).PostConfigure(null, options);

        options.ApiKey.ShouldBe("from-json");
    }

    [Theory]
    [InlineData("set-cloud-key", true)]
    [InlineData("--set-cloud-key", true)]
    [InlineData("CLEAR-CLOUD-KEY", true)]
    [InlineData("--environment=Production", false)]
    public void IsCommand_RecognisesTheTwoSwitches(string arg, bool expected) =>
        CloudKeyCommand.IsCommand([arg]).ShouldBe(expected);

    [Fact]
    public void IsCommand_NoArguments_IsTheService() => CloudKeyCommand.IsCommand([]).ShouldBeFalse();

    [Fact]
    public void SetCommand_ReadsTheKeyFromTheConsoleAndStoresIt()
    {
        var output = new StringWriter();

        var exit = CloudKeyCommand.Run(["set-cloud-key"], _store, new StringReader(" piped-key \n"), output);

        exit.ShouldBe(CloudKeyCommand.Ok);
        _store.GetApiKey().ShouldBe("piped-key");
        output.ToString().ShouldNotContain("piped-key");
    }

    [Fact]
    public void SetCommand_EmptyInput_ChangesNothing()
    {
        _store.SaveApiKey("old-key");

        var exit = CloudKeyCommand.Run(["set-cloud-key"], _store, new StringReader("\n"), new StringWriter());

        exit.ShouldBe(CloudKeyCommand.EmptyKey);
        _store.GetApiKey().ShouldBe("old-key");
    }

    [Fact]
    public void ClearCommand_RemovesTheStoredKey()
    {
        _store.SaveApiKey("old-key");

        var exit = CloudKeyCommand.Run(["clear-cloud-key"], _store, TextReader.Null, new StringWriter());

        exit.ShouldBe(CloudKeyCommand.Ok);
        _store.GetApiKey().ShouldBeNull();
    }
}
