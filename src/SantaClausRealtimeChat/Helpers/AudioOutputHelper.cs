using NAudio.Wave;

namespace SantaClausRealtimeChat.Helpers;

/// <summary>
///     Helper class for managing audio output using NAudio library.
/// </summary>
internal class AudioOutputHelper : IDisposable
{
    private readonly BufferedWaveProvider _waveProvider;
    private readonly WaveOutEvent _waveOutEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AudioOutputHelper"/> class.
    ///     Sets up the audio format and initializes the wave provider and output event.
    /// </summary>
    public AudioOutputHelper()
    {
        WaveFormat outputAudioFormat = new(
            rate: 24000,
            bits: 16,
            channels: 1);

        _waveProvider = new(outputAudioFormat)
        {
            BufferDuration = TimeSpan.FromMinutes(2),
        };

        _waveOutEvent = new();
        _waveOutEvent.Init(_waveProvider);
        _waveOutEvent.Play();
    }

    /// <summary>
    ///     Enqueues audio data for playback.
    /// </summary>
    /// <param name="audioData">The audio data to be played back.</param>
    public void EnqueueForPlayback(BinaryData audioData)
    {
        if (audioData is null)
        {
            return;
        }

        byte[] buffer = audioData.ToArray();
        _waveProvider.AddSamples(buffer, 0, buffer.Length);
    }

    /// <summary>
    ///     Clears the playback buffer.
    /// </summary>
    public void ClearPlayback()
    {
        _waveProvider.ClearBuffer();
    }

    /// <summary>
    ///     Disposes the resources used by the <see cref="AudioOutputHelper"/> class.
    /// </summary>
    public void Dispose()
    {
        _waveOutEvent?.Dispose();
        GC.SuppressFinalize(this);
    }
}
