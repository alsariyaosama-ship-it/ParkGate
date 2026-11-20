using ParkGate.Infrastructure.Sync;

namespace ParkGate.SyncAgent;

/// <summary>
/// Command-line switches to manage the cloud API key (R8.3):
/// <c>ParkGate.SyncAgent.exe set-cloud-key</c> reads the key from the console (typed or piped, never from the command
/// line, so it does not land in the shell history) and stores it with DPAPI; <c>clear-cloud-key</c> removes it.
/// </summary>
public static class CloudKeyCommand
{
    public const string SetCommand = "set-cloud-key";
    public const string ClearCommand = "clear-cloud-key";

    public const int Ok = 0;
    public const int EmptyKey = 2;

    public static bool IsCommand(string[] args) =>
        args.Length > 0 && (Is(args[0], SetCommand) || Is(args[0], ClearCommand));

    public static int Run(string[] args, ICloudApiKeyStore store, TextReader input, TextWriter output, Func<string?>? readHidden = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (Is(args[0], ClearCommand))
        {
            output.WriteLine(store.Clear() ? "Cloud API key removed." : "No cloud API key was stored.");
            return Ok;
        }

        output.Write("Cloud API key: ");
        var key = (readHidden is null ? input.ReadLine() : readHidden())?.Trim();
        output.WriteLine();
        if (string.IsNullOrEmpty(key))
        {
            output.WriteLine("No key entered; nothing changed.");
            return EmptyKey;
        }

        store.SaveApiKey(key);
        output.WriteLine("Cloud API key saved (DPAPI, this machine only). Restart the ParkGate SyncAgent service.");
        output.WriteLine("Remove any \"CloudApi:ApiKey\" from appsettings.local.json.");
        return Ok;
    }

    /// <summary>Reads a line without echoing it when a person types it; piped input is read as it is.</summary>
    public static string? ReadHiddenFromConsole()
    {
        if (Console.IsInputRedirected)
        {
            return Console.In.ReadLine();
        }

        var buffer = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                return buffer.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.Length--;
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }
    }

    private static bool Is(string arg, string command) =>
        string.Equals(arg.TrimStart('-', '/'), command, StringComparison.OrdinalIgnoreCase);
}
