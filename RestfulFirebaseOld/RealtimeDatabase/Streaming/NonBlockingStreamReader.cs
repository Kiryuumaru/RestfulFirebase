﻿using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace RestfulFirebase.RealtimeDatabase.Streaming;

internal class NonBlockingStreamReader : TextReader
{
    private const int DefaultBufferSize = 16000;

    private readonly Stream stream;
    private readonly byte[] buffer;
    private readonly int bufferSize;

    private string cachedData;

    public NonBlockingStreamReader(Stream stream, int bufferSize = DefaultBufferSize)
    {
        this.stream = stream;
        this.bufferSize = bufferSize;
        buffer = new byte[bufferSize];

        cachedData = string.Empty;
    }

    public override async Task<string?> ReadLineAsync()
    {
        string? currentString = TryGetNewLine();

        while (currentString == null)
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            var read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize)).ConfigureAwait(false);
#else
            var read = await stream.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
#endif
            var str = Encoding.UTF8.GetString(buffer, 0, read);

            cachedData += str;
            currentString = TryGetNewLine();
        }

        return currentString;
    }

    public async Task<string?> ReadLineAsync(CancellationToken token)
    {
        string? currentString = TryGetNewLine();

        while (currentString == null)
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            var read = await stream.ReadAsync(buffer.AsMemory(0, bufferSize), token).ConfigureAwait(false);
#else
            var read = await stream.ReadAsync(buffer, 0, bufferSize, token).ConfigureAwait(false);
#endif
            var str = Encoding.UTF8.GetString(buffer, 0, read);

            cachedData += str;
            currentString = TryGetNewLine();
        }

        return currentString;
    }

    private string? TryGetNewLine()
    {
        var newLine = cachedData.IndexOf('\n');

        if (newLine >= 0)
        {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            var r = cachedData[..(newLine + 1)];
#else
            var r = cachedData[..(newLine + 1)];
#endif
            cachedData = cachedData.Remove(0, r.Length);
            return r.Trim();
        }

        return null;
    }
}
