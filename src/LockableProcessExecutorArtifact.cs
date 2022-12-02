using System.Text;

namespace Kenet.SimpleProcess;

internal class SealableProcessExecutorArtifact : IProcessExecutorMutator
{
    private readonly ProcessExecutorArtifact _artifact;
    private readonly List<Action<ProcessExecution>> _onRunCallbacks = new List<Action<ProcessExecution>>();
    private bool _isSealed;

    public SealableProcessExecutorArtifact(ProcessExecutorArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    private void ThrowIfSealed()
    {
        if (_isSealed) {
            throw new InvalidOperationException("This process executor artifact has been already sealed");
        }
    }

    private void Mutate(Action changeArtifact)
    {
        lock (this) {
            ThrowIfSealed();
            changeArtifact();
        }
    }

    internal (ProcessExecutorArtifact Artifact, IEnumerable<Action<ProcessExecution>> OnRunCallbacks) Seal()
    {
        lock (this) {
            _isSealed = true;
            return (_artifact, _onRunCallbacks);
        }
    }

    public void WithExitCode(Func<int, bool> validator) =>
        Mutate(() => _artifact.WithExitCode(validator));

    public void WithErrorInterpretation(Encoding? encoding) =>
        Mutate(() => _artifact.WithErrorInterpretation(encoding));

    public void AddErrorWriter(WriteHandler writer) =>
        Mutate(() => _artifact.AddErrorWriter(writer));

    public void AddOutputWriter(WriteHandler writer) =>
        Mutate(() => _artifact.AddOutputWriter(writer));

    public void AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        Mutate(() => _artifact.AddCancellation(cancellationTokens));

    public void OnRun(Action<ProcessExecution> callback) =>
        Mutate(() => _onRunCallbacks.Add(callback));
}
