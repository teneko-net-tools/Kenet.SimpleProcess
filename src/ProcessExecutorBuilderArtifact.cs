using System.Text;

namespace Kenet.SimpleProcess;

internal class ProcessExecutorBuilderArtifact : IProcessExecutorArtifact
{
    public SimpleProcessStartInfo StartInfo { get; }
    public IReadOnlyCollection<CancellationToken> CancellationTokens { get; }
    public IReadOnlyCollection<WriteHandler> ErrorWriters { get; }
    public IReadOnlyCollection<WriteHandler> OutputWriters { get; }
    public Encoding? ExitErrorEncoding { get; }
    public Func<int, bool>? ValidateExitCode { get; }

    public ProcessExecutorBuilderArtifact(
        SimpleProcessStartInfo startInfo,
        IReadOnlyCollection<CancellationToken> cancellationTokens,
        IReadOnlyCollection<WriteHandler> errorTracers,
        IReadOnlyCollection<WriteHandler> outputTracers,
        Encoding? exitErrorEncoding,
        Func<int, bool>? validateExitCode)
    {
        StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
        CancellationTokens = cancellationTokens ?? throw new ArgumentNullException(nameof(cancellationTokens));
        ErrorWriters = errorTracers ?? throw new ArgumentNullException(nameof(errorTracers));
        OutputWriters = outputTracers ?? throw new ArgumentNullException(nameof(outputTracers));
        ExitErrorEncoding = exitErrorEncoding;
        ValidateExitCode = validateExitCode;
    }
}
