using System.Media;

namespace ParkGate.WinForms.Services;

/// <summary>Short cues for the desk and the gift screen (R5.7). Callers check the settings first.</summary>
public interface IDeskSounds
{
    /// <summary>Entry recorded, or gift redeemed.</summary>
    void PlayGranted();

    /// <summary>Entry or gift refused.</summary>
    void PlayDenied();
}

/// <summary>
/// Plays two tones generated in code instead of <see cref="SystemSounds"/>, which depend on the Windows sound scheme
/// (often silent or the same sound for both). No sound files ship, so there is nothing to license or lose.
/// </summary>
public sealed class DeskSounds : IDeskSounds, IDisposable
{
    /// <summary>Two rising notes: a short, friendly "done".</summary>
    internal static readonly byte[] GrantedWave = ToneWave.Create((880, 90), (1320, 140));

    /// <summary>Two low notes: clearly different from the success cue.</summary>
    internal static readonly byte[] DeniedWave = ToneWave.Create((330, 160), (0, 60), (247, 260));

    private readonly SoundPlayer _granted = Load(GrantedWave);
    private readonly SoundPlayer _denied = Load(DeniedWave);

    public void PlayGranted() => Play(_granted);

    public void PlayDenied() => Play(_denied);

    public void Dispose()
    {
        _granted.Dispose();
        _denied.Dispose();
    }

    private static SoundPlayer Load(byte[] wave)
    {
        var player = new SoundPlayer(new MemoryStream(wave, writable: false));
        player.Load();
        return player;
    }

    private static void Play(SoundPlayer player)
    {
        try
        {
            player.Play(); // asynchronous; a new cue stops the previous one
        }
        catch (InvalidOperationException)
        {
            // No audio device: a missing cue must never block the desk.
        }
    }
}

/// <summary>Builds a 16-bit mono PCM WAVE file from (frequency Hz, duration ms) notes; 0 Hz is a rest.</summary>
public static class ToneWave
{
    public const int SampleRate = 22050;

    private const short Amplitude = 9000;
    private const int FadeMilliseconds = 8; // short ramps so notes do not click
    private const int HeaderBytes = 44;

    public static byte[] Create(params (double Hz, int Milliseconds)[] notes)
    {
        var samples = notes.Sum(n => SampleRate * n.Milliseconds / 1000);
        var data = samples * 2;
        using var stream = new MemoryStream(HeaderBytes + data);
        using var w = new BinaryWriter(stream);

        w.Write("RIFF"u8);
        w.Write(HeaderBytes - 8 + data);
        w.Write("WAVE"u8);
        w.Write("fmt "u8);
        w.Write(16);                 // fmt chunk size
        w.Write((short)1);           // PCM
        w.Write((short)1);           // mono
        w.Write(SampleRate);
        w.Write(SampleRate * 2);     // bytes per second
        w.Write((short)2);           // block align
        w.Write((short)16);          // bits per sample
        w.Write("data"u8);
        w.Write(data);

        var fade = SampleRate * FadeMilliseconds / 1000;
        foreach (var (hz, ms) in notes)
        {
            var count = SampleRate * ms / 1000;
            for (var i = 0; i < count; i++)
            {
                var envelope = Math.Min(1.0, Math.Min(i, count - 1 - i) / (double)fade);
                var value = hz <= 0 ? 0 : Math.Sin(2 * Math.PI * hz * i / SampleRate) * Amplitude * envelope;
                w.Write((short)Math.Round(value));
            }
        }

        w.Flush();
        return stream.ToArray();
    }
}
