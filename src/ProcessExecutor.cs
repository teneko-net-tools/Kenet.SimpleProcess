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
    IRunnable<IContextlessProcessExecution>,
    IRunnable<IAsyncProcessExecution>,
    IRunnable<IAsyncContextlessProcessExecution>,
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
    public ProcessExecutor WithExitCode(Func<int, bool> validator)
    {
        ((IProcessExecutorMutator)_artifact).WithExitCode(validator);
        return this;
    }

    void IProcessExecutorMutator.WithExitCode(Func<int, bool> validator) =>
        ((IProcessExecutorMutator)_artifact).WithExitCode(validator);

    /// <inheritdoc cref="IProcessExecutorMutator.WithErrorInterpretation(Encoding?)"/>
    public ProcessExecutor WithErrorInterpretation(Encoding? encoding)
    {
        ((IProcessExecutorMutator)_artifact).WithErrorInterpretation(encoding);
        return this;
    }

    void IProcessExecutorMutator.WithErrorInterpretation(Encoding? encoding) =>
        ((IProcessExecutorMutator)_artifact).WithErrorInterpretation(encoding);

    /// <inheritdoc cref="IProcessExecutorMutator.AddErrorWriter(WriteHandler)"/>
    public ProcessExecutor AddErrorWriter(WriteHandler writer)
    {
        ((IProcessExecutorMutator)_artifact).AddErrorWriter(writer);
        return this;
    }

    void IProcessExecutorMutator.AddErrorWriter(WriteHandler writer) =>
        ((IProcessExecutorMutator)_artifact).AddErrorWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddOutputWriter(WriteHandler)"/>
    public ProcessExecutor AddOutputWriter(WriteHandler writer)
    {
        ((IProcessExecutorMutator)_artifact).AddOutputWriter(writer);
        return this;
    }

    void IProcessExecutorMutator.AddOutputWriter(WriteHandler writer) =>
        ((IProcessExecutorMutator)_artifact).AddOutputWriter(writer);

    /// <inheritdoc cref="IProcessExecutorMutator.AddCancellation(IEnumerable{CancellationToken})"/>
    public ProcessExecutor AddCancellation(IEnumerable<CancellationToken> cancellationTokens)
    {
        ((IProcessExecutorMutator)_artifact).AddCancellation(cancellationTokens);
        return this;
    }

    void IProcessExecutorMutator.AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        ((IProcessExecutorMutator)_artifact).AddCancellation(cancellationTokens);

    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines, Encoding encoding, ProcessBoundary boundary)
    {
        var lineStream = new AsyncLineStream();

        _ = readFrom(this)(bytes => {
            if (bytes.IsEmpty) {
                lineStream.Complete();
                return;
            }

            var memoryOwner = MemoryPool<byte>.Shared.Rent(bytes.Length);
            bytes.CopyTo(memoryOwner.Memory.Span);
            lineStream.Write(memoryOwner);
        });

        async IAsyncEnumerable<string> CreateAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var newCancellationToken = cancellationTokenSource.Token;

            Disposables.Delegated? cancellationOnBoundaryExceeding = null;
            ProcessBoundaryRegistration boundaryAssociation = default;

            try {
                if (!boundary.IsFaulted) {
                    cancellationOnBoundaryExceeding = new Disposables.Delegated(cancellationTokenSource.Cancel);
                    _ = newCancellationToken.Register(cancellationOnBoundaryExceeding.Revoke);
                    boundaryAssociation = boundary.Associate(cancellationOnBoundaryExceeding);
                }

                var execution = await _waitForExecution.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                using var cancelOnExit = execution.Cancelled.Register(cancellationTokenSource.Cancel);

                while (await lineStream.WrittenLines.OutputAvailableAsync().ConfigureAwait(false)) {
                    using var bytesOwner = await lineStream.WrittenLines.TakeAsync().ConfigureAwait(false);
                    yield return encoding.GetString(bytesOwner.Memory.ToArray());
                }
            } finally {
                cancellationOnBoundaryExceeding?.Dispose();
                boundaryAssociation.Dispose();
            }
        }

        asyncLines = CreateAsyncEnumerable();
        return this;
    }

    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines, Encoding encoding) =>
        WriteToAsyncLines(readFrom, out asyncLines, encoding, ProcessBoundary.Faulted);

    public ProcessExecutor WriteToAsyncLines(Func<ProcessExecutor, Func<WriteHandler, object>> readFrom, out IAsyncEnumerable<string> asyncLines) =>
        WriteToAsyncLines(readFrom, out asyncLines, Encoding.UTF8, ProcessBoundary.Faulted);

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

    IContextlessProcessExecution IRunnable<IContextlessProcessExecution>.Run() =>
        Run();

    IAsyncProcessExecution IRunnable<IAsyncProcessExecution>.Run() =>
        Run();

    IAsyncContextlessProcessExecution IRunnable<IAsyncContextlessProcessExecution>.Run() =>
        Run();
}
