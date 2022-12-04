namespace Kenet.SimpleProcess;

internal static class CancellationTokenSourceExtensions
{
    public static void TryCancel(this CancellationTokenSource cancellationTokenSource)
    {
        try {
            cancellationTokenSource.Cancel();
        } catch (ObjectDisposedException) {
            ; // Ignore by purpose
        }
    }
}
