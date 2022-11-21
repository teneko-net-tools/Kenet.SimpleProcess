namespace Kenet.SimpleProcess;

public interface IRunnable<T>
{
    /// <summary>
    /// Runs something that the implementer of <see cref="IRunnable{T}"/> is owning.
    /// </summary>
    /// <returns>An object giving you the chance to control the flow of just ran object.</returns>
    public T Run();
}
