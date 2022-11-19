using System.Diagnostics;
#if !NET5_0_OR_GREATER
using Nito.AsyncEx;
#endif

namespace Kenet.SimpleProcess;

internal static class ProcessExtensions
{
    public static Task WaitForExitAsync(this System.Diagnostics.Process process, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return process.WaitForExitAsync(cancellationToken);
#else
        return process.WaitForExitAsyncFallback(cancellationToken);
#endif
    }

#if !NET5_0_OR_GREATER
    public static async Task WaitForExitAsyncFallback(this System.Diagnostics.Process process, CancellationToken cancellationToken)
    {
        if (!process.HasExited) {
            cancellationToken.ThrowIfCancellationRequested();
        }

        try {
            process.EnableRaisingEvents = true;
        } catch (InvalidOperationException) {
            if (process.HasExited) {
                return;
            }

            throw;
        }

        var exitTaskSource = new TaskCompletionSource<object?>();

        void OnExited(object? sender, EventArgs args)
        {
            exitTaskSource.TrySetResult(null);
        }

        process.Exited += OnExited;

        try {
            if (process.HasExited) {
                return;
            }

            await exitTaskSource.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        } finally {
            process.Exited -= OnExited;
        }
    }
#endif
}
