namespace Kenet.SimpleProcess;

/// <summary>
/// Represents the "boundary" of a process. When you decide to dispose this boundary, then all instances that got associated with this boundary are disposed too.
/// </summary>
public readonly struct ProcessBoundary : IDisposable
{
    private const int _defaultInitialCapacity = 4;

    private static readonly List<IDisposable?> _disposedIndicatingList = new(capacity: 0);
    private static readonly List<IDisposable?> _invalidatedIndicatingList = new(capacity: 1);

    /// <summary>
    /// Represents a boundary that is already disposed.
    /// </summary>
    public static readonly ProcessBoundary Disposed = new ProcessBoundary(initialCapacity: 0);

    /// <summary>
    /// Represents a special boundary that is in a faulty state. Any attempt to add an association or to dispsoe the instance results into an <see cref="InvalidOperationException"/>.
    /// </summary>
    internal static readonly ProcessBoundary Faulted = new ProcessBoundary(initialCapacity: 1);

    internal static void DisposeOrFailSilently(IDisposable? instance)
    {
        try {
            instance?.Dispose();
        } catch (ObjectDisposedException) {
            ; // We ignore on purpose
        }
    }

    /// <summary>
    /// Indicates whether the boundary has been disposed or not.
    /// </summary>
    public bool IsDisposed =>
        _buffer.Capacity == 0;

    /// <summary>
    /// Indicates whether the boundary has been invalidated or not.
    /// </summary>
    /// <remarks>
    /// If <see langword="true"/> it is not allowed to associate further disposables with this boundary.
    /// </remarks>
    internal bool IsFaulted =>
        _buffer.Capacity == 1;

    private readonly List<IDisposable?> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessBoundary"/> class.
    /// </summary>
    /// <param name="initialCapacity">The minimum capacity with which to initialize the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCapacity"/> is not valid.</exception>
    private ProcessBoundary(int initialCapacity) =>
        _buffer = initialCapacity == 0
            ? _disposedIndicatingList
            : (initialCapacity == 1
                ? _invalidatedIndicatingList
                : new List<IDisposable?>(initialCapacity));

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessBoundary"/> class.
    /// </summary>
    public ProcessBoundary() : this(_defaultInitialCapacity)
    {
    }

    private void EnsureNotFaulted()
    {
        if (IsFaulted) {
            throw new InvalidOperationException("The underlying buffer indicates an invalidated state");
        }
    }

    /// <summary>
    /// Associates <paramref name="disposable"/> with this boundary.
    /// When this struct is getting disposed, the given disposable
    /// will be disposed too.
    /// </summary>
    /// <param name="disposable"></param>
    public ProcessBoundaryRegistration Associate(IDisposable disposable)
    {
        EnsureNotFaulted();

        lock (_buffer) {
            if (IsDisposed) {
                disposable.Dispose();
                return ProcessBoundaryRegistration.Default;
            } else {
                var start = _buffer.Count;
                _buffer.Add(disposable);
                return new ProcessBoundaryRegistration(this, start, length: 1);
            }
        }
    }

    /// <summary>
    /// Associates <paramref name="disposables"/> with this boundary.
    /// When this struct is getting disposed, the given disposables
    /// will be disposed too.
    /// </summary>
    /// <param name="disposables"></param>
    /// <exception cref="AggregateException"></exception>
    public ProcessBoundaryRegistration Associate(IEnumerable<IDisposable> disposables)
    {
        EnsureNotFaulted();

        lock (_buffer) {
            if (IsDisposed) {
                Disposables.DisposeRange(_buffer, out var aggregatedError);

                if (aggregatedError != null) {
                    throw aggregatedError;
                }

                return ProcessBoundaryRegistration.Default;
            } else {
                var start = _buffer.Count;
                _buffer.AddRange(disposables);
                var postAddLength = _buffer.Count;

                return new ProcessBoundaryRegistration(
                    this,
                    start,
                    length: postAddLength - start);
            }
        }
    }

    /// <summary>
    /// Associates <paramref name="disposables"/> with this boundary.
    /// When this struct is getting disposed, the given disposables
    /// will be disposed too.
    /// </summary>
    /// <param name="disposables"></param>
    /// <exception cref="AggregateException"></exception>
    public ProcessBoundaryRegistration Associate(params IDisposable[] disposables) =>
        Associate((IEnumerable<IDisposable>)disposables);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal void DisableAssociations(int start, int length)
    {
        lock (_buffer) {
            if (IsDisposed) {
                return;
            }

            if (_buffer[start] == null) {
                return;
            }

            for (var i = start; i < length; i++) {
                _buffer[i] = null;
            }
        }
    }

    /// <summary>
    /// Disposes everything this struct is associated with.
    /// </summary>
    /// <exception cref="AggregateException"></exception>
    public void Dispose()
    {
        EnsureNotFaulted();

        lock (_buffer) {
            if (IsDisposed) {
                return;
            }

            Disposables.DisposeRange(_buffer, out var aggregatedError);

            _buffer.Clear();
            _buffer.Capacity = 0;

            if (aggregatedError != null) {
                throw aggregatedError;
            }
        }
    }
}
