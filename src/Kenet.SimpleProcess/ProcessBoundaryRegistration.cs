namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the association registration. In case of disposal the association will be removed from the underlying <see cref="ProcessBoundary"/>.
/// </summary>
public readonly struct ProcessBoundaryRegistration : IDisposable
{
    /// <summary>
    /// Represents the default instance of this type.
    /// </summary>
    public static readonly ProcessBoundaryRegistration Default = new ProcessBoundaryRegistration();

    private readonly ProcessBoundary _boundary;
    private readonly int _start;
    private readonly int _length;

    internal ProcessBoundaryRegistration(ProcessBoundary boundary, int start, int length)
    {
        _boundary = boundary;
        _start = start;
        _length = length;
    }

    /// <summary>
    /// Creates a association registration without functionality.
    /// </summary>
    public ProcessBoundaryRegistration()
    {
        _boundary = ProcessBoundary.Disposed;
        _start = 0;
        _length = 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_length == 0) {
            return;
        }

        _boundary.DisableAssociations(_start, _length);
    }
}
