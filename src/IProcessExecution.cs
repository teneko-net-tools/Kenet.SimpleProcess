namespace Kenet.SimpleProcess;

public interface IProcessExecution
{
    CancellationToken Exited { get; }
}
