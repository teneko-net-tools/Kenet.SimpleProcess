namespace Kenet.SimpleProcess;

public partial class SimpleProcess
{
    private static readonly TimerCallback s_timerCallback = new TimerCallback(OnTimerCallback);

    private static void OnTimerCallback(object? obj)
    {
        var process = (SimpleProcess)obj!;

        if (!process.IsDisposed) {
            process.Cancel();
        }
    }

    private volatile Timer? _timer;

    /// <inheritdoc/>
    public void CancelAfter(int millisecondsDelay)
    {
        if (IsDisposed) {
            return;
        }

        if (millisecondsDelay < -1) {
            throw new ArgumentOutOfRangeException(nameof(millisecondsDelay));
        }

        if (IsCompleted) {
            return;
        }

        if (_timer == null) {
            var newTimer = new Timer(s_timerCallback, this, -1, -1);

            if (Interlocked.CompareExchange(ref _timer, newTimer, null) != null) {
                newTimer.Dispose();
            }
        }

        try {
            _timer.Change(millisecondsDelay, -1);
        } catch (ObjectDisposedException) {
            ; // Ignored on purpose
        }
    }

    /// <inheritdoc/>
    public void CancelAfter(TimeSpan delay)
    {
        var totalMilliseconds = (long)delay.TotalMilliseconds;

        if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue) {
            throw new ArgumentOutOfRangeException(nameof(delay));
        }

        CancelAfter((int)totalMilliseconds);
    }
}
