using Mix.Heart.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mix.Heart.Exceptions
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException()
        {
        }

        public HttpResponseException(string message) : base(message)
        {
        }

        public HttpResponseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HttpResponseException(MixErrorStatus status, params string[] messages)
        {
            Status = status;
            Value= messages;
        }

        protected HttpResponseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MixErrorStatus Status { get; set; } = MixErrorStatus.ServerError;

        public object Value { get; set; }
    }
}
