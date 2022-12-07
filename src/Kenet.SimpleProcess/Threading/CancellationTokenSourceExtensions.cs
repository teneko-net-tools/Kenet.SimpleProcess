namespace Kenet.SimpleProcess.Threading;

internal static class CancellationTokenSourceExtensions
{
    public static void TryCancel(this CancellationTokenSource cancellationTokenSource, CancellationErrorDiscardHandling errorDiscardHandling)
    {
        try {
            cancellationTokenSource.Cancel();
        } catch (ObjectDisposedException) {
            if (!errorDiscardHandling.HasFlag(CancellationErrorDiscardHandling.ObjectDisposed)) {
                throw;
            }
        } catch (AggregateException) {
            if (!errorDiscardHandling.HasFlag(CancellationErrorDiscardHandling.AggregatedError)) {
                throw;
            }
        } catch (Exception) {
            if (!errorDiscardHandling.HasFlag(CancellationErrorDiscardHandling.Others)) {
                throw;
            }
        }
    }

    public static void TryCancel(this CancellationTokenSource cancellationTokenSource) =>
        cancellationTokenSource.TryCancel(CancellationErrorDiscardHandling.All);
}
