using Newtonsoft.Json;

namespace Mix.Heart.Models
{
    /// <summary>
    ///
    /// </summary>
    public class EntityField
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        [JsonProperty("propertyName")]
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>
        /// The property value.
        /// </value>
        [JsonProperty("propertyValue")]
        public object PropertyValue { get; set; }
    }
}
