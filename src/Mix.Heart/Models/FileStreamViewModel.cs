using Newtonsoft.Json;

namespace Mix.Heart.Models
{
    /// <summary>
    ///
    /// </summary>
    public class FileStreamViewModel
    {
        /// <summary>
        /// Gets or sets the base64.
        /// </summary>
        /// <value>
        /// The base64.
        /// </value>
        [JsonProperty("base64")]
        public string Base64 { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
