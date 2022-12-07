namespace Kenet.SimpleProcess.Threading;

[Flags]
internal enum CancellationErrorDiscardHandling
{
    None = 0,
    ObjectDisposed = 1,
    AggregatedError = 2,
    Others = 4,
    All = ObjectDisposed | AggregatedError | Others
}
