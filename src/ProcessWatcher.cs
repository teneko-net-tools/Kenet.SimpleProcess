using System.Diagnostics;

namespace Kenet.SimpleProcess;

internal class ProcessWatcher : IDisposable
{
    public CancellationToken Exited { get; }
    public bool IsDisposed => _isDisposed == 1;
    public bool IsExited => Exited.IsCancellationRequested;

    private readonly Process _watchableProcess;
    private readonly CancellationTokenSource _processExitedTokenSource;
    private Process? _watchingProcess;
    private int _isDisposed;

    public ProcessWatcher(Process watchableProcess)
    {
        _watchableProcess = watchableProcess ?? throw new ArgumentNullException(nameof(watchableProcess));
        _processExitedTokenSource = new CancellationTokenSource();
        Exited = _processExitedTokenSource.Token;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="watchOptions"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void Watch(ProcessWatchOptions watchOptions)
    {
        if (_watchingProcess != null) {
            return;
        }

        lock (this) {
            if (_watchingProcess != null) {
                return;
            }

            if (IsDisposed) {
                return;
            }

            var processId = _watchableProcess.Id;

            try {
                _watchingProcess = Process.GetProcessById(processId);
                _watchingProcess.EnableRaisingEvents = true;

                void Exited(object? sender, EventArgs args)
                {
                    _watchingProcess.Exited -= Exited;
                    _processExitedTokenSource.Cancel();
                }

                _watchingProcess.Exited += Exited;
            } catch (ArgumentException) {
                if (watchOptions.HasFlag(ProcessWatchOptions.ExitedIfNotFound)) {
                    _processExitedTokenSource.Cancel();
                } else {
                    throw;
                }
            } catch (InvalidOperationException error) when (error.Message.Contains($"({processId}) has exited")) {
                _processExitedTokenSource.Cancel();
            }
        }
    }

    public void Watch() =>
        Watch(ProcessWatchOptions.None);

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1) {
            return;
        }

        _watchingProcess?.Dispose();
        _processExitedTokenSource.Dispose();
    }
}
