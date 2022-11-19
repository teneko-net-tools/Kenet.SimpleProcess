using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Kenet.SimpleProcess;

/// <summary>
/// A simple process trying to replace the bootstrap code of a native instance of <see cref="System.Diagnostics.Process" />
/// .
/// </summary>
public sealed class SimpleProcess : IProcess, IAsyncProcess
{
    private static async Task ReadStreamAsync(
        Stream source,
        WriteHandler writeNextBytes,
        CancellationToken cancellationToken)
    {
        var lastWrittenBytesCount = -1;

        try {
            while (!(cancellationToken.IsCancellationRequested && lastWrittenBytesCount == 0)) {
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(1024 * 4);
                lastWrittenBytesCount =
                    await source.ReadAsync(memoryOwner.Memory, cancellationToken).ConfigureAwait(false);

                if (lastWrittenBytesCount != 0) {
                    writeNextBytes(memoryOwner.Memory.Span[..lastWrittenBytesCount]);
                }
            }
        } catch (OperationCanceledException) {
            // This should not be handled here
        }
    }

    /// <summary>
    /// <see langword="true" /> if internal process has been started.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_readOutputTask), nameof(_readErrorTask))]
    public bool IsProcessStarted { get; private set; }

    /// <summary>
    /// <see langword="true" /> if internal process has been started successfully.
    /// </summary>
    public bool IsProcessStartedSuccessfully { get; private set; }

    /// <summary>
    /// A token that gets cancelled when the process exits.
    /// </summary>
    public CancellationToken Exited { get; }

    private readonly CancellationToken _userCancellationToken;
    private Task? _readOutputTask;
    private Task? _readErrorTask;

    private readonly WriteHandler? _errorWriter;
    private readonly WriteHandler? _outputWriter;

    private readonly CancellationTokenSource _processExitedSource;
    private readonly SimpleProcessStartInfo _processStartInfo;
    private readonly object _startProcessLock = new();
    private bool _isDisposed;
    private Process? _process;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="startInfo"></param>
    /// <param name="outputWriter">The buffer the process will write incoming output to.</param>
    /// <param name="errorWriter">The buffer the process will write incoming error to.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SimpleProcess(
        SimpleProcessStartInfo startInfo,
        WriteHandler? outputWriter,
        WriteHandler? errorWriter,
        CancellationToken cancellationToken)
    {
        _userCancellationToken = cancellationToken;
        _processStartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
        _outputWriter = outputWriter;
        _errorWriter = errorWriter;
        _processExitedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Exited = _processExitedSource.Token;
    }

    internal void CreateProcess(out Process process)
    {
        var currentProcess = _process;

        if (currentProcess is not null) {
            process = currentProcess;
            return;
        }

        var newProcess = new Process { StartInfo = _processStartInfo.CreateProcessStartInfo() };

        if (Interlocked.CompareExchange(ref _process, newProcess, null) == null) {
            process = newProcess;
            return;
        }

        // Someone else was faster, so we dispose new process
        newProcess.Dispose();
        process = currentProcess!;
    }

    [MemberNotNull(nameof(_readOutputTask), nameof(_readErrorTask))]
    private void StartProcess(Process process)
    {
        // Try non-lock version
        if (IsProcessStarted) {
            return;
        }

        lock (_startProcessLock) {
            if (IsProcessStarted) {
                return;
            }

            process.EnableRaisingEvents = true;

            void OnProcessExited(object? sender, EventArgs e)
            {
                process.Exited -= OnProcessExited;
                _processExitedSource.Cancel();
            }

            process.Exited += OnProcessExited;

            IsProcessStartedSuccessfully = process.Start();
            IsProcessStarted = true;

            _readOutputTask = _outputWriter is not null
                ? ReadStreamAsync(process.StandardOutput.BaseStream, _outputWriter, Exited)
                : Task.CompletedTask;

            _readErrorTask = _errorWriter is not null
                ? ReadStreamAsync(process.StandardError.BaseStream, _errorWriter, Exited)
                : Task.CompletedTask;
        }
    }

    /// <inheritdoc cref="IProcess.Start" />
    /// />
    public bool Start()
    {
        CreateProcess(out var process);
        StartProcess(process);
        return IsProcessStartedSuccessfully;
    }

    /// <inheritdoc />
    public int WaitForExit()
    {
        CreateProcess(out var process);
        StartProcess(process);

        // This produces potentially a never ending task, but should
        // end if the process exited.
        var waitForExitTask = Task.Run(() => process.WaitForExit(), _userCancellationToken);

        // This makes the assumption, that every await uses
        // ConfigureAwait(continueOnCapturedContext: false)
        Task.WaitAll(new[] { waitForExitTask, _readErrorTask, _readErrorTask }, _userCancellationToken);

        // REMINDER: Do not kill the process if user has requested the
        // cancellation, because this is not scope of this method
        return process.ExitCode;
    }

    /// <inheritdoc />
    public async Task<int> WaitForExitAsync()
    {
        CreateProcess(out var process);
        StartProcess(process);

        await Task.WhenAll(
                process.WaitForExitAsync(_userCancellationToken),
                _readOutputTask,
                _readErrorTask)
            .ConfigureAwait(false);

        return process.ExitCode;
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed) {
            return;
        }

        if (disposing) {
            _process?.Dispose();
            _processExitedSource.Dispose();
        }

        _isDisposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
