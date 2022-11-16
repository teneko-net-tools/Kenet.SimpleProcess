using System.Runtime.Serialization;

namespace Kenet.SimpleProcess
{
    /// <summary>
    /// An exception that indicates a bad exit code.
    /// </summary>
    public class BadExitCodeException : Exception
    {
        /// <summary>
        /// The bad exit code.
        /// </summary>
        public int? ExitCode { get; init; }

        /// <inheritdoc/>
        public override string Message => (base.Message?.Length == 0 ? "The process exited with bad exit code" : base.Message)
           + (ExitCode != null ? $"{Environment.NewLine}Exit Code = {ExitCode}" : string.Empty);

        /// <inheritdoc/>
        public BadExitCodeException()
        {
        }

        /// <inheritdoc/>
        public BadExitCodeException(string? message) : base(message)
        {
        }

        /// <inheritdoc/>
        public BadExitCodeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected BadExitCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
