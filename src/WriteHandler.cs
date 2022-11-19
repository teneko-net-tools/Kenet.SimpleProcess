namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the handler to write incoming bytes. These bytes are only available during the invoke of
/// <see cref="WriteHandler" />.
/// </summary>
/// <param name="bytes"></param>
public delegate void WriteHandler(ReadOnlySpan<byte> bytes);
