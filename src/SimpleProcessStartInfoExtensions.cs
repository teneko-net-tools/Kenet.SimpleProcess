namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="SimpleProcessStartInfo"/>.
/// </summary>
public static class SimpleProcessStartInfoExtensions
{
    /// <summary>
    /// Builds an instance of <see cref="ProcessExecutorBuilder"/> from <paramref name="startInfo"/>.
    /// </summary>
    /// <param name="startInfo"></param>
    public static ProcessExecutorBuilder ToExecutorBuilder(this SimpleProcessStartInfo startInfo) =>
        new ProcessExecutorBuilder(startInfo);

    /// <inheritdoc cref="ProcessExecutorBuilder.CreateDefault(SimpleProcessStartInfo, int)"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this SimpleProcessStartInfo startInfo, int validExitCode) =>
        ProcessExecutorBuilder.CreateDefault(startInfo, validExitCode);

    /// <inheritdoc cref="ProcessExecutorBuilder.CreateDefault(SimpleProcessStartInfo, Func{int, bool})"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this SimpleProcessStartInfo startInfo, Func<int, bool> validateExitCode) =>
        ProcessExecutorBuilder.CreateDefault(startInfo, validateExitCode);

    /// <inheritdoc cref="ProcessExecutorBuilder.CreateDefault(SimpleProcessStartInfo)"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this SimpleProcessStartInfo startInfo) =>
        ProcessExecutorBuilder.CreateDefault(startInfo);
}
