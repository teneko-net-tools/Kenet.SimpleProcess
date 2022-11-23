namespace Kenet.SimpleProcess;

public interface IContextlessProcessExecution : IRunningProcess
{
    /// <summary>
    /// Instructs the process to wait for the associated process to exit.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that can cancel the on-going promise of completion. When this cancellation token is the origin of cancellation,
    /// then please take into account that the current execution is not yet disposed because the process is still executing.
    /// This allows you to try for another completion.
    /// </param>
    /// <param name="completionOptions">Specifies options while waiting for completion.</param>
    int RunToCompletion(CancellationToken cancellationToken, ProcessCompletionOptions completionOptions);
}
