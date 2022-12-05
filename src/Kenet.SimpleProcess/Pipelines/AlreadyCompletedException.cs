using System.Runtime.Serialization;

namespace Kenet.SimpleProcess.Pipelines;

[Serializable]
internal class AlreadyCompletedException : Exception
{
    public AlreadyCompletedException()
    {
    }

    public AlreadyCompletedException(string? message) : base(message)
    {
    }

    public AlreadyCompletedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected AlreadyCompletedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
