namespace Kenet.SimpleProcess;

internal static class Disposables
{
    internal static void DisposeRange(IEnumerable<IDisposable?> disposables, out AggregateException? aggregatedError)
    {
        var exceptions = new List<Exception>();

        foreach (var disposable in disposables) {
            try {
                disposable?.Dispose();
            } catch (Exception error) {
                exceptions.Add(error);
            }
        }

        aggregatedError = exceptions.Count == 0
            ? null
            : new AggregateException(exceptions);
    }

    internal sealed class Empty : IDisposable
    {
        public static readonly Empty Instance = new Empty();

        private Empty()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class Delegated : IDisposable
    {
        private Action? _action;

        public Delegated(Action action) =>
            _action = action ?? throw new ArgumentNullException(nameof(action));

        public void Revoke() =>
            _action = null;

        public void Dispose() =>
            _action?.Invoke();
    }
}
