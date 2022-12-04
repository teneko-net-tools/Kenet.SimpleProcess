﻿namespace Kenet.SimpleProcess;

/// <summary>
/// Extension methods for <see cref="ProcessStartInfoBuilder"/>.
/// </summary>
public static class ProcessStartInfoBuilderExtensions
{
    /// <summary>
    /// Builds an instance of <see cref="ProcessExecutorBuilder"/> from <paramref name="startInfoBuilder"/>.
    /// </summary>
    /// <param name="startInfoBuilder"></param>
    public static ProcessExecutorBuilder ToExecutorBuilder(this ProcessStartInfoBuilder startInfoBuilder) =>
        startInfoBuilder.Build().ToExecutorBuilder();

    /// <inheritdoc cref="SimpleProcessStartInfoExtensions.ToDefaultExecutorBuilder(SimpleProcessStartInfo, int)"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this ProcessStartInfoBuilder startInfoBuilder, int validExitCode) =>
        startInfoBuilder.Build().ToDefaultExecutorBuilder(validExitCode);

    /// <inheritdoc cref="SimpleProcessStartInfoExtensions.ToDefaultExecutorBuilder(SimpleProcessStartInfo, Func{int, bool})"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this ProcessStartInfoBuilder startInfoBuilder, Func<int, bool> validateExitCode) =>
        startInfoBuilder.Build().ToDefaultExecutorBuilder(validateExitCode);

    /// <inheritdoc cref="SimpleProcessStartInfoExtensions.ToDefaultExecutorBuilder(SimpleProcessStartInfo)"/>
    public static ProcessExecutorBuilder ToDefaultExecutorBuilder(this ProcessStartInfoBuilder startInfoBuilder) =>
        startInfoBuilder.Build().ToDefaultExecutorBuilder();

    /// <summary>
    /// Sets the arguments and escapes them depending on running OS plattform.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="unescapedArguments"></param>
    public static ProcessStartInfoBuilder WithOSIndependentArguments(this ProcessStartInfoBuilder builder, IEnumerable<string> unescapedArguments) =>
        builder.WithArguments(SimpleProcessStartInfo.EscapeArguments(unescapedArguments));

    /// <inheritdoc cref="WithOSIndependentArguments(ProcessStartInfoBuilder, IEnumerable{string})"/>    
    public static ProcessStartInfoBuilder WithOSIndependentArguments(this ProcessStartInfoBuilder builder, params string[] unescapedArguments) =>
        builder.WithOSIndependentArguments((IEnumerable<string>)unescapedArguments);
}
