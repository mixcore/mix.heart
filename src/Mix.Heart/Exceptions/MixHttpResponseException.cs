using Mix.Heart.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mix.Heart.Exceptions
{
    public class MixHttpResponseException : Exception
    {
        public MixHttpResponseException()
        {
        }

        public MixHttpResponseException(string message) : base(message)
        {
        }

        public MixHttpResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MixHttpResponseException(MixErrorStatus status, params string[] messages) : base()
        {
            Status = status;
            Value = messages;
        }

        protected MixHttpResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MixErrorStatus Status { get; set; } = MixErrorStatus.ServerError;

        public object Value { get; set; }
    }
}
