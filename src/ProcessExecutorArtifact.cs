using System.Text;

namespace Kenet.SimpleProcess;

internal class ProcessExecutorArtifact : IProcessExecutorArtifact
{
    public SimpleProcessStartInfo StartInfo { get; }
    public List<CancellationToken> CancellationTokens { get; }
    IReadOnlyCollection<CancellationToken> IProcessExecutorArtifact.CancellationTokens => CancellationTokens;
    public List<WriteHandler> ErrorWriters { get; }
    IReadOnlyCollection<WriteHandler> IProcessExecutorArtifact.ErrorWriters => ErrorWriters;
    public List<WriteHandler> OutputWriters { get; }
    IReadOnlyCollection<WriteHandler> IProcessExecutorArtifact.OutputWriters => OutputWriters;
    public Encoding? ExitErrorEncoding { get; internal set; }
    public Func<int, bool>? ValidateExitCode { get; internal set; }

    public ProcessExecutorArtifact(
        SimpleProcessStartInfo startInfo,
        List<CancellationToken> cancellationTokens,
        List<WriteHandler> errorTracers,
        List<WriteHandler> outputTracers,
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

    public ProcessExecutorArtifact(SimpleProcessStartInfo startInfo)
    {
        StartInfo = startInfo ?? throw new ArgumentNullException(nameof(startInfo));
        CancellationTokens = new();
        ErrorWriters = new();
        OutputWriters = new();
    }
}
