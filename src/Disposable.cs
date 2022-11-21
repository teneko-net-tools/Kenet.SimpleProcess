namespace Kenet.SimpleProcess;

internal class Disposable
{
    internal static void TryDisposeInstance(IDisposable? instance)
    {

        try {
            instance?.Dispose();
        } catch (ObjectDisposedException) {
            ; // We ignore on purpose
        }
    }
}
