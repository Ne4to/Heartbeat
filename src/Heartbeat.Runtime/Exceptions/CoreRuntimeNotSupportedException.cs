using System;

namespace Heartbeat.Runtime.Exceptions
{
    public class CoreRuntimeNotSupportedException : Exception
    {
        public CoreRuntimeNotSupportedException()
        {
        }

        public CoreRuntimeNotSupportedException(string message) : base(message)
        {
        }

        public CoreRuntimeNotSupportedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}