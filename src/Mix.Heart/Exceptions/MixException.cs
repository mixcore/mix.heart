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

        public MixException(MixErrorStatus status, params object[] messages) : base(string.Join('\n', messages))
        {
            Status = status;
            Value = messages;
        }

        public MixException(MixErrorStatus status, Exception ex) : base(ex.InnerException?.Message ?? ex.Message)
        {
            Status = status;
            Value = ex.Data;
        }

        public MixErrorStatus Status { get; set; } = MixErrorStatus.ServerError;

        public object Value { get; set; }
        public string[] Errors { get => Message?.Split('\n') ?? new string[] { }; }
    }
}
