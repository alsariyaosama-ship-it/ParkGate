using System.Runtime.Versioning;
using ParkGate.Infrastructure.Security;

namespace ParkGate.Infrastructure.Tests.Security;

[SupportedOSPlatform("windows")]
public sealed class DpapiConnectionSettingsStoreTests : IDisposable
{
    private readonly string _tempFile;
    private readonly DpapiConnectionSettingsStore _store;

    public DpapiConnectionSettingsStoreTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"parkgate_test_conn_{Guid.NewGuid():N}.dat");
        _store = new DpapiConnectionSettingsStore(_tempFile);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            try
            {
                File.Delete(_tempFile);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    [Fact]
    public void Exists_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        _store.Exists().ShouldBeFalse();
        _store.GetConnectionString().ShouldBeNull();
    }

    [Fact]
    public void SaveAndGetConnectionString_RoundTrip_ShouldMatchOriginal()
    {
        var originalConnStr = "Server=prod-server;Database=ParkGate_Prod;User Id=app;Password=VerySecret123!";

        _store.SaveConnectionString(originalConnStr);

        _store.Exists().ShouldBeTrue();
        var retrieved = _store.GetConnectionString();
        retrieved.ShouldBe(originalConnStr);
    }

    [Fact]
    public void Clear_ShouldDeleteFile()
    {
        _store.SaveConnectionString("Server=test;Database=db;");
        _store.Exists().ShouldBeTrue();

        _store.Clear();

        _store.Exists().ShouldBeFalse();
        _store.GetConnectionString().ShouldBeNull();
    }

    [Fact]
    public void SaveConnectionString_WithEmptyString_ShouldThrowArgumentException()
    {
        Should.Throw<ArgumentException>(() => _store.SaveConnectionString("   "));
    }
}
