using System.Runtime.CompilerServices;

namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the "boundary" of a process. When you decide to dispose this boundary, then all instances that got associated with this boundary are disposed too.
/// </summary>
public readonly struct ProcessBoundary : IDisposable
{
    private static readonly List<IDisposable> _emptyList = new List<IDisposable>();

    /// <summary>
    /// Represents a boundary that is already disposed.
    /// </summary>
    public static readonly ProcessBoundary Invalidated = new ProcessBoundary(initialCapacity: 0);

    private const int _defaultInitialBufferSize = 4;

    internal static void DisposeOrFailSilently(IDisposable? instance)
    {
        try {
            instance?.Dispose();
        } catch (ObjectDisposedException) {
            ; // We ignore on purpose
        }
    }

    private readonly List<IDisposable> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessBoundary"/> class.
    /// </summary>
    /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is not valid.</exception>
    private ProcessBoundary(int initialCapacity) =>
        _buffer = initialCapacity == 0
            ? _emptyList
            : new List<IDisposable>(initialCapacity);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessBoundary"/> class.
    /// </summary>
    public ProcessBoundary() : this(_defaultInitialBufferSize)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsBufferInvalidated() =>
        _buffer.Capacity == 0;

    private void EnsureBufferNotInvalidated()
    {
        if (IsBufferInvalidated()) {
            throw new ObjectDisposedException(objectName: null, "The underlying buffer has been already disposed");
        }
    }

    /// <summary>
    /// Associates <paramref name="disposable"/> with this boundary.
    /// When this struct is getting disposed, the given disposable
    /// will be disposed too.
    /// </summary>
    /// <param name="disposable"></param>
    public void Associate(IDisposable disposable)
    {
        lock (_buffer) {
            EnsureBufferNotInvalidated();
            _buffer.Add(disposable);
        }
    }

    /// <summary>
    /// Associates <paramref name="disposables"/> with this boundary.
    /// When this struct is getting disposed, the given disposables
    /// will be disposed too.
    /// </summary>
    /// <param name="disposables"></param>
    public void Associate(IEnumerable<IDisposable> disposables)
    {
        lock (_buffer) {
            EnsureBufferNotInvalidated();
            _buffer.AddRange(disposables);
        }
    }

    /// <summary>
    /// Associates <paramref name="disposables"/> with this boundary.
    /// When this struct is getting disposed, the given disposables
    /// will be disposed too.
    /// </summary>
    /// <param name="disposables"></param>
    public void Associate(params IDisposable[] disposables)
    {
        lock (_buffer) {
            EnsureBufferNotInvalidated();
            _buffer.AddRange(disposables);
        }
    }

    /// <summary>
    /// Disposes everything this struct is associated with.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        lock (_buffer) {
            if (IsBufferInvalidated()) {
                return;
            }

            _buffer.Clear();
            _buffer.Capacity = 0;
            _buffer.TrimExcess();
        }
    }
}
