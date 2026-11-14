using System.Data.Common;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

/// <summary>R4.7: the operator never sees raw exception text; connection failures say so and suggest a retry.</summary>
public sealed class UserMessagesTests
{
    private const string Secret = "Server=SECRET-SQL;Password=hunter2";

    private sealed class FakeDbException(string message) : DbException(message);

    public static TheoryData<Exception> ConnectionFailures() =>
    [
        new FakeDbException(Secret),
        new TimeoutException(Secret),
        new SocketException(10061),
        new InvalidOperationException("wrapper", new FakeDbException(Secret)),
        new AggregateException(new TimeoutException(Secret)),
    ];

    [Theory]
    [MemberData(nameof(ConnectionFailures))]
    public void Describe_ConnectionFailure_SaysConnectionAndRetryWithoutTheExceptionText(Exception ex)
    {
        var message = UserMessages.Describe(ex);

        message.ShouldContain(Strings.Banner_ConnectionFailed);
        message.ShouldContain(Strings.Banner_ConnectionRetryHint);
        message.ShouldNotContain("SECRET");
    }

    [Fact]
    public void Describe_AnyOtherException_IsTheGenericMessage()
    {
        UserMessages.Describe(new NullReferenceException(Secret)).ShouldBe(Strings.Msg_UnexpectedError);
    }

    [Fact]
    public void WinFormsCode_NeverShowsExceptionMessageText()
    {
        var root = FindRepoRoot();
        var offenders = Directory
            .EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => Regex.IsMatch(File.ReadAllText(f), @"\b(ex|exception)\.Message\b"))
            .Select(f => Path.GetRelativePath(root, f))
            .ToList();

        offenders.ShouldBeEmpty("Show UserMessages.Describe(ex) and log the exception instead of its Message.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ParkGate.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("ParkGate.sln not found above the test output.");
    }
}
