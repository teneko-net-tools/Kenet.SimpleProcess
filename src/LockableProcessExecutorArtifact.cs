using System.Text;

namespace Kenet.SimpleProcess;

internal class SealableProcessExecutorArtifact : IProcessExecutorMutator
{
    private readonly ProcessExecutorArtifact _artifact;
    volatile bool _isSealed;

    public SealableProcessExecutorArtifact(ProcessExecutorArtifact artifact) =>
        _artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));

    private void ThrowIfSealed()
    {
        if (_isSealed) {
            throw new InvalidOperationException("This process executor artifact has been already sealed");
        }
    }

    private IProcessExecutorMutator Mutate(Action<IProcessExecutorMutator> changeArtifact)
    {
        lock (this) {
            ThrowIfSealed();
            changeArtifact(_artifact);
        }

        return this;
    }

    public void Seal(out ProcessExecutorArtifact artifact)
    {
        lock (this) {
            _isSealed = true;
            artifact = _artifact;
        }
    }

    public void WithExitCode(Func<int, bool> validator) =>
        Mutate(artifact => artifact.WithExitCode(validator));

    public void WithErrorInterpretation(Encoding? encoding) =>
        Mutate(artifact => artifact.WithErrorInterpretation(encoding));

    public void AddErrorWriter(WriteHandler writer) =>
        Mutate(artifact => artifact.AddErrorWriter(writer));

    public void AddOutputWriter(WriteHandler writer) =>
        Mutate(artifact => artifact.AddOutputWriter(writer));

    public void AddCancellation(IEnumerable<CancellationToken> cancellationTokens) =>
        Mutate(artifact => artifact.AddCancellation(cancellationTokens));
}
