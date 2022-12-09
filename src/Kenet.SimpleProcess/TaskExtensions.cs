namespace Kenet.SimpleProcess;

internal static class TaskExtensions
{
    public static Task ContinueWithIfNotCompletedSuccessfully(this Task task, Action action) =>
        task.ContinueWith(
            continuedTask => {
                if (continuedTask.IsFaulted || continuedTask.IsCanceled) {
                    action();
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Current); // I think TaskScheduler.Current should be okay; it inherits the previous task's task scheduler
}
