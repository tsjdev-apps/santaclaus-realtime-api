using NAudio.Wave;

namespace SantaClausRealtimeChat.Helpers;

/// <summary>
///     Helper class for handling audio input using NAudio library.
/// </summary>
internal class AudioInputHelper : Stream, IDisposable
{
    private const int SAMPLES_PER_SECOND = 24000;
    private const int BYTES_PER_SAMPLE = 2;
    private const int CHANNELS = 1;

    private readonly byte[] _buffer 
        = new byte[BYTES_PER_SAMPLE * SAMPLES_PER_SECOND * CHANNELS * 10];

    private readonly Lock _bufferLock 
        = new();

    private int _bufferReadPos 
        = 0;

    private int _bufferWritePos 
        = 0;

    private readonly WaveInEvent _waveInEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AudioInputHelper"/> class.
    /// </summary>
    private AudioInputHelper()
    {
        _waveInEvent = new()
        {
            WaveFormat = new WaveFormat(
                SAMPLES_PER_SECOND, 
                BYTES_PER_SAMPLE * 8, 
                CHANNELS),
        };

        _waveInEvent.DataAvailable += (_, e) =>
        {
            lock (_bufferLock)
            {
                int bytesToCopy = e.BytesRecorded;
                if (_bufferWritePos + bytesToCopy 
                        >= _buffer.Length)
                {
                    int bytesToCopyBeforeWrap 
                        = _buffer.Length - _bufferWritePos;
                    
                    Array.Copy(
                        e.Buffer, 
                        0,
                        _buffer, 
                        _bufferWritePos, 
                        bytesToCopyBeforeWrap);

                    bytesToCopy -= bytesToCopyBeforeWrap;
                    _bufferWritePos = 0;
                }

                Array.Copy(
                    e.Buffer,
                    e.BytesRecorded - bytesToCopy, 
                    _buffer, 
                    _bufferWritePos, 
                    bytesToCopy);

                _bufferWritePos += bytesToCopy;
            }
        };

        _waveInEvent.StartRecording();
    }

    /// <summary>
    ///     Starts the audio input helper.
    /// </summary>
    /// <returns>A new instance of <see cref="AudioInputHelper"/>.</returns>
    public static AudioInputHelper Start()
        => new();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool CanRead
        => true;

    /// <inheritdoc/>
    public override bool CanSeek
        => false;

    /// <inheritdoc/>
    public override bool CanWrite
        => false;

    /// <inheritdoc/>
    public override long Length
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public override void SetLength(long value)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public override void Flush()
        => throw new NotImplementedException();

    /// <summary>
    ///     Reads a sequence of bytes from the current stream and advances the 
    ///     position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">An array of bytes. 
    /// When this method returns, the buffer contains the specified byte array with the 
    /// values between offset and (offset + count - 1) replaced by the bytes read from 
    /// the current source.</param>
    /// <param name="offset">The zero-based byte offset in buffer at which to begin storing 
    /// the data read from the current stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <returns>The total number of bytes read into the buffer. This can be less than the 
    /// number of bytes requested if that many bytes are not currently available, or zero 
    /// if the end of the stream has been reached.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalCount = count;

        int GetBytesAvailable()
            => _bufferWritePos < _bufferReadPos
                ? _bufferWritePos + (_buffer.Length - _bufferReadPos)
                : _bufferWritePos - _bufferReadPos;

        while (GetBytesAvailable() < count)
        {
            Thread.Sleep(100);
        }

        lock (_bufferLock)
        {
            if (_bufferReadPos + count >= _buffer.Length)
            {
                int bytesBeforeWrap = _buffer.Length - _bufferReadPos;

                Array.Copy(
                    sourceArray: _buffer,
                    sourceIndex: _bufferReadPos,
                    destinationArray: buffer,
                    destinationIndex: offset,
                    length: bytesBeforeWrap);

                _bufferReadPos = 0;

                count -= bytesBeforeWrap;
                offset += bytesBeforeWrap;
            }

            Array.Copy(_buffer, _bufferReadPos, buffer, offset, count);

            _bufferReadPos += count;
        }

        return totalCount;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the 
    /// <see cref="AudioInputHelper"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; 
    /// false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        _waveInEvent?.Dispose();
        base.Dispose(disposing);
    }
}
