namespace Kenet.SimpleProcess;

internal class CompositeWriter
{
    private readonly IEnumerable<WriteHandler> _writers;

    public CompositeWriter(IEnumerable<WriteHandler> writers) =>
        _writers = writers;

    public void Write(ReadOnlySpan<byte> bytes)
    {
        foreach (var write in _writers) {
            // We should not disturb other write handlers
            try {
                write(bytes);
            } catch {
                ; // Ignore on purpose
            }
        }
    }
}
