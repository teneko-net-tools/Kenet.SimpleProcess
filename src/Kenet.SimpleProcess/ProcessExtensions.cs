#if !NET5_0_OR_GREATER
using Nito.AsyncEx;
#endif

using System.Diagnostics;

namespace Kenet.SimpleProcess;

internal static partial class ProcessExtensions
{
#if !NET5_0_OR_GREATER
    private static async Task WaitForExitAsyncFallback(this Process process, CancellationToken cancellationToken)
    {
        try {
            process.EnableRaisingEvents = true;
        } catch (InvalidOperationException) {
            if (process.HasExited) {
                return;
            }

            throw;
        }

        var exitTaskSource = new TaskCompletionSource<object?>();

        using var cancelExitTask = cancellationToken.Register(
            () => exitTaskSource.TrySetException(new OperationCanceledException("The operation to wait asynchronously for the process exit has been cancelled", cancellationToken)),
            useSynchronizationContext: false);

        void OnExited(object? sender, EventArgs args) =>
            exitTaskSource.TrySetResult(null);

        process.Exited += OnExited;

        try {
            if (process.HasExited) {
                return;
            }

            await exitTaskSource.Task.ConfigureAwait(false);
        } finally {
            process.Exited -= OnExited;
        }
    }
#endif

    public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return process.WaitForExitAsync(cancellationToken);
#else
        return process.WaitForExitAsyncFallback(cancellationToken);
#endif
    }
}
