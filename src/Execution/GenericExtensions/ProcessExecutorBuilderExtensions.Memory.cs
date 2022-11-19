namespace Kenet.SimpleProcess.Execution.GenericExtensions;

public static partial class ProcessExecutorBuilderExtensions
{
    public static T WriteTo<T>(
        this T builder,
        Func<T, Func<WriteHandler, object>> readFrom,
        Memory<byte> memory,
        int startIndex)
        where T : IProcessExecutorBuilder
    {
        var writtenBytes = startIndex;

        readFrom(builder)(bytes => {
            bytes.CopyTo(memory.Span.Slice(writtenBytes, bytes.Length));
            checked { writtenBytes += bytes.Length; }
        });

        return builder;
    }

    public static T WriteTo<T>(
        this T builder,
        Func<T, Func<WriteHandler, object>> readFrom,
        Memory<byte> memory)
        where T : IProcessExecutorBuilder =>
        builder.WriteTo(readFrom, memory, startIndex: 0);
}
