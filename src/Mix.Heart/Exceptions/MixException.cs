using Mix.Heart.Enums;
using System;
using System.Runtime.Serialization;

namespace Mix.Heart.Exceptions
{
    public class MixException : Exception
    {
        public MixException()
        {
        }

        public MixException(string message) : base(message)
        {
        }

        public MixException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        protected MixException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MixException(MixErrorStatus status, params object[] messages) : base()
        {
            Status = status;
            Value = messages;
        }

        public MixErrorStatus Status { get; set; } = MixErrorStatus.ServerError;

        public object Value { get; set; }
    }
}
