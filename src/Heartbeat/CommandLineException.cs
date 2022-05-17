using System.Runtime.Serialization;

namespace Heartbeat.Hosting.Console
{
    internal class CommandLineException : Exception
    {
        public CommandLineException()
        {
        }

        protected CommandLineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public CommandLineException(string? message)
            : base(message)
        {
        }

        public CommandLineException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}