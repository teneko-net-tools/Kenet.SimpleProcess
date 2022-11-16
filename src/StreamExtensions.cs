namespace Kenet.SimpleProcess;

internal static class StreamExtensions
{
#if !NET6_0_OR_GREATER
    /// <inheritdoc cref="CommunityToolkit.HighPerformance.StreamExtensions.ReadAsync(Stream, Memory{byte}, CancellationToken)"/>
    public static ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        CommunityToolkit.HighPerformance.StreamExtensions.ReadAsync(stream, buffer, cancellationToken);
#endif
}
