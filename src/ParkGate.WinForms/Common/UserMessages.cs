using System.Data.Common;
using System.Net.Sockets;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Turns an unexpected exception into the Arabic text the operator sees (R4.7). The exception's own message is
/// never shown: it is often English, may name the server or the connection, and means nothing at the desk.
/// Callers log the exception; <see cref="LogAndDescribe"/> does both for classes without an injected logger.
/// </summary>
public static class UserMessages
{
    public static string Describe(Exception exception) =>
        IsConnectionFailure(exception)
            ? $"{Strings.Banner_ConnectionFailed}. {Strings.Banner_ConnectionRetryHint}"
            : Strings.Msg_UnexpectedError;

    /// <summary>Logs through the application's Serilog logger, then returns <see cref="Describe"/>.</summary>
    public static string LogAndDescribe(Exception exception, string context)
    {
        Serilog.Log.Error(exception, "Unexpected error: {Context}", context);
        return Describe(exception);
    }

    /// <summary>True when the database or the network is unreachable, anywhere in the exception chain.</summary>
    public static bool IsConnectionFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is DbException or TimeoutException or SocketException)
            {
                return true;
            }

            if (current is AggregateException aggregate && aggregate.InnerExceptions.Any(IsConnectionFailure))
            {
                return true;
            }
        }

        return false;
    }
}
