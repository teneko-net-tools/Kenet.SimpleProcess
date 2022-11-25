namespace Kenet.SimpleProcess;

[Flags]
internal enum ProcessWatchOptions
{
    None = 0,
    ExitedIfNotFound = 1
}
