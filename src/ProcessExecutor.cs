using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Kenet.SimpleProcess.Pipelines;
using Nito.AsyncEx;

namespace Kenet.SimpleProcess;

/// <summary>
/// A repeatable process executor.
/// </summary>
public sealed class ProcessExecutor :
    IRunnable<IProcessExecution>,
    IRunnable<ICompletableProcessExecution>,
    IRunnable<IAsyncProcessExecution>,
    IRunnable<IAsyncCompletableProcessExecution>,
    IProcessExecutorMutator
{
    private readonly SealableProcessExecutorArtifact _artifact;
    private ProcessExecution? _execution;
    private TaskCompletionSource<ProcessExecution> _waitForExecution;

    /// <summary>
    /// Creates an instance of this type by providing an artifact.
    /// </summary>
    /// <param name="artifact">
    /// The artifact to be used to spawn the process execution.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    internal ProcessExecutor(ProcessExecutorArtifact artifact)
    {
        if (artifact is null) {
            throw new ArgumentNullException(nameof(artifact));
        }

        _artifact = new SealableProcessExecutorArtifact(artifact);
        _waitForExecution = new TaskCompletionSource<ProcessExecution>();
    }

    /// <summary>
    /// Creates an instance of this type by providing an artifact.
    /// </summary>
    /// <param name="artifact">
    /// The artifact to be used to spawn the process execution.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ProcessExecutor(IProcessExecutorArtifact artifact)
        : this(artifact.Clone())
    {
    }

    /// <inheritdoc cref="IProcessExecutorMutator.WithExitCode(Func{int, bool})"/>
    /// <remarks>
    /// Once you called <see cref="Run"/>, the executor gets sealed and prevents any further mutation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// This process executor artifact has been already sealed.
    /// </exception>
    public ProcessExecutor WithExitCode(Func<int, bool> validator)
    {
        ((IProcessExecutorMutator)_artifact).WithExitCode(validator);
        return this;
    }

    void IProcessExecutorMutator.WithExitCode(Func<int, bool> validator) =>
        ((IProcessExecutorMutator)_artifact).WithExitCode(validator);

    /// <inheritdoc cref="IProcessExecutorMutator.WithErrorInterpretation(Encoding?)"/>
    /// <inheritdoc cref="WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public ProcessExecutor WithErrorInterpretation(Encoding? encoding)
    {
        ((IProcessExecutorMutator)_artifact).WithErrorInterpretation(encoding);
        return this;
    }

    void IProcessExecutorMutator.WithErrorInterpretation(Encoding? encoding) =>
        ((IProcessExecutorMutator)_artifact).WithErrorInterpretation(encoding);

    /// <inheritdoc cref="IProcessExecutorMutator.AddErrorWriter(WriteHandler)"/>
    /// <inheritdoc cref="WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public ProcessExecutor AddErrorWriter(WriteHandler writer)
    {
        ((IProcessExecutorMutator)_artifact).AddErrorWriter(writer);
        return this;
    }

    void IProcessExecutorMutator.AddErrorWriter(WriteHandler writer) =>
        ((IProcessExecutorMutator)_artifact).AddErrorWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddOutputWriter(WriteHandler)"/>
    /// <inheritdoc cref="WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public ProcessExecutor AddOutputWriter(WriteHandler writer)
    {
        ((IProcessExecutorMutator)_artifact).AddOutputWriter(writer);
        return this;
    }

    void IProcessExecutorMutator.AddOutputWriter(WriteHandler writer) =>
        ((IProcessExecutorMutator)_artifact).AddOutputWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddCancellation(IEnumerable{CancellationToken})"/>
    /// <inheritdoc cref="WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public ProcessExecutor AddCancellation(IEnumerable<CancellationToken> cancellationTokens)
    {
        ((IProcessExecutorMutator)_artifact).AddCancellation(cancellationTokens);
        return this;
    }

    void IProcessExecutorMutator.AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        ((IProcessExecutorMutator)_artifact).AddCancellation(cancellationTokens);

    /// <summary>
    /// Writes internally to a stream that gets asynchronously redirected to <paramref name="asyncLines"/>.
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="asyncLines">
    /// Represents a pipeline that asynchronously waits for incoming bytes, decodes them on-the-fly and returns lines as soon as they are appearing.
    /// </param>
    /// <param name="encoding">The decoder used for incoming bytes.</param>
    /// <param name="boundary">
    /// If the process boundary gets disposed, then the current or next cancellable operation will be cancelled.
    /// </param>
    /// <inheritdoc cref="WithExitCode(Func{int, bool})" path="/*[position()>last()-2]"/>
    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines, Encoding encoding, ProcessBoundary boundary)
    {
        CancellationTokenSource cancellationTokenSource = null!; // CreateAsyncEnumerable() is 100% called
        var isCancellationTokenSourceDisposed = false;
        var lineStream = new AsyncLineStream();

        _ = readFrom(this)(bytes => {
            if (lineStream.IsCompleted || lineStream.IsDisposed) {
                return;
            }

            IMemoryOwner<byte>? memoryOwner = null;
            void DisposeMemory() => memoryOwner?.Dispose();

            try {
                if (bytes.IsEndOfStream()) {
                    lineStream.Complete();
                    return;
                }

                memoryOwner = MemoryPool<byte>.Shared.Rent(bytes.Length);
                bytes.CopyTo(memoryOwner.Memory.Span);
                lineStream.Write(new ConsumedMemoryOwner<byte>(memoryOwner, bytes.Length));
            } catch (Exception error) {
                DisposeMemory();

                if (error is ObjectDisposedException || error is AlreadyCompletedException) {
                    return;
                }

                lock (cancellationTokenSource) {
                    // In case this handler gets called once again after
                    // the async enumerable may have been already exited
                    if (!isCancellationTokenSourceDisposed) {
                        cancellationTokenSource.Cancel();
                    }
                }
            }
        });

        async IAsyncEnumerable<string> CreateAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var linkedCancellationToken = cancellationTokenSource.Token;

            Disposables.Delegated? cancellationOnBoundaryExceeding = null;
            ProcessBoundaryRegistration boundaryAssociation = default;

            try {
                if (!boundary.IsFaulted) {
                    cancellationOnBoundaryExceeding = new Disposables.Delegated(cancellationTokenSource.Cancel);
                    _ = linkedCancellationToken.Register(cancellationOnBoundaryExceeding.Revoke);
                    boundaryAssociation = boundary.Associate(cancellationOnBoundaryExceeding);
                }

                var execution = await _waitForExecution.Task.WaitAsync(linkedCancellationToken).ConfigureAwait(false);
                using var cancelOnProcessCancellation = execution.Cancelled.Register(cancellationTokenSource.Cancel);

                // REMINDER: OutputAvailableAsync() won't throw OperationCanceledException, it will just cancel
                while (await lineStream.WrittenLines.OutputAvailableAsync(linkedCancellationToken).ConfigureAwait(false)) {
                    // When taking, no no blocking wait will be involved due to previous OutputAvailableAsync()
                    using var bytesOwner = lineStream.WrittenLines.Take();
                    yield return encoding.GetString(bytesOwner.ConsumedMemory.Span, bytesOwner.ConsumedCount);
                }
            } finally {
                lineStream.Dispose();
                cancellationOnBoundaryExceeding?.Dispose();
                boundaryAssociation.Dispose();

                lock (cancellationTokenSource) {
                    cancellationTokenSource.Dispose();
                    isCancellationTokenSourceDisposed = true;
                }
            }
        }

        asyncLines = CreateAsyncEnumerable();
        return this;
    }

    /// <inheritdoc cref="WriteToAsyncLines(Func{ProcessExecutor, Func{WriteHandler, object}}, out IAsyncEnumerable{string}, Encoding, ProcessBoundary)"/>
    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines, Encoding encoding) =>
        WriteToAsyncLines(readFrom, out asyncLines, encoding, ProcessBoundary.Faulted);

    /// <inheritdoc cref="WriteToAsyncLines(Func{ProcessExecutor, Func{WriteHandler, object}}, out IAsyncEnumerable{string}, Encoding, ProcessBoundary)"/>
    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines) =>
        WriteToAsyncLines(readFrom, out asyncLines, Encoding.UTF8, ProcessBoundary.Faulted);

    /// <inheritdoc cref="WriteToAsyncLines(Func{ProcessExecutor, Func{WriteHandler, object}}, out IAsyncEnumerable{string}, Encoding, ProcessBoundary)"/>
    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines, ProcessBoundary boundary) =>
        WriteToAsyncLines(readFrom, out asyncLines, Encoding.UTF8, boundary);

    /// <inheritdoc cref="SimpleProcess.Run()"/>
    public ProcessExecution Run()
    {
        lock (_artifact) {
            var currentExecution = _execution;

            if (currentExecution != null) {
                return currentExecution;
            }

            _artifact.Seal(out var artifact);
            var newExecution = ProcessExecution.Create(artifact);
            _waitForExecution.SetResult(newExecution);
            newExecution.Run();
            return newExecution;
        }
    }

    IProcessExecution IRunnable<IProcessExecution>.Run() =>
        Run();

    ICompletableProcessExecution IRunnable<ICompletableProcessExecution>.Run() =>
        Run();

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();

    IAsyncCompletableProcessExecution IRunnable<IAsyncCompletableProcessExecution>.Run() =>
        Run();
}
