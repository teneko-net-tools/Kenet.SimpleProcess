using System.Runtime.Serialization;

namespace Kenet.SimpleProcess;

internal class ProcessReuseException : Exception
{
    public ProcessReuseException()
    {
    }

    public ProcessReuseException(string message) : base(message)
    {
    }

    public ProcessReuseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ProcessReuseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
