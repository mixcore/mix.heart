using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mix.Heart.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class RepositoryResponse<TResult>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is succeed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is succeed; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("isSucceed")]
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Gets or sets the response key.
        /// </summary>
        /// <value>
        /// The response key.
        /// </value>
        [JsonProperty("responseKey")]
        public string ResponseKey { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        [JsonProperty("errors")]
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        [JsonProperty("exception")]
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty("data")]
        public TResult Data { get; set; }

        [JsonProperty("lastUpdateConfiguration")]
        public DateTime? LastUpdateConfiguration { get; set; }
    }
}
