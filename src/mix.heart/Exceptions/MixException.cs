using Mix.Heart.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
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
            LogException(message: message);
        }
         public MixException(MixErrorStatus status) : base()
        {
            Status = status;
            LogException(status: status, message: Message);
        }

        protected MixException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            LogException(message: Message);
        }

        public MixException(MixErrorStatus status, object description, params object[] messages) : base(string.Join('\n', messages))
        {
            Status = status;
            Value = messages;
            LogException(status: status, message: Message);
        }

        public MixException(MixErrorStatus status, Exception ex) : base(ex.InnerException?.Message ?? ex.Message)
        {
            Status = status;
            Value = ex.Data;
            LogException(ex);
        }

        public MixErrorStatus Status { get; set; } = MixErrorStatus.ServerError;

        public object Value { get; set; }
        public string[] Errors { get => Message?.Split('\n') ?? new string[] { }; }

        private void LogException(Exception ex = null, MixErrorStatus? status = null, string message = null)
        {
            Console.Error.Write(ex);

            string fullPath = $"{Environment.CurrentDirectory}/logs/{DateTime.Now:dd-MM-yyyy}";
            if (!string.IsNullOrEmpty(fullPath) && !Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            string filePath = $"{fullPath}/log_exceptions.json";

            try
            {
                FileInfo file = new(filePath);
                string content = "[]";
                if (file.Exists)
                {
                    using (StreamReader s = file.OpenText())
                    {
                        content = s.ReadToEnd();
                    }
                    File.Delete(filePath);
                }

                JArray arrExceptions = JArray.Parse(content);
                JObject jex = new()
                {
                    new JProperty("CreatedDateTime", DateTime.UtcNow),
                    new JProperty("Status", status?.ToString()),
                    new JProperty("Message", message),
                    new JProperty("Details", ex == null ? null : JObject.FromObject(ex))
                };
                arrExceptions.Add(jex);
                content = arrExceptions.ToString();

                using var writer = File.CreateText(filePath);
                writer.WriteLine(content);
            }
            catch
            {
                Console.Write($"Cannot write log file {filePath}");
                // File invalid
            }
        }
    }
}
