// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;

namespace Mix.Domain.Core.Models
{
    /// <summary>
    /// Supported Culture
    /// </summary>
    public class SupportedCulture
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the specificulture.
        /// </summary>
        /// <value>
        /// The specificulture.
        /// </value>
        [JsonProperty("specificulture")]
        public string Specificulture { get; set; }

        /// <summary>
        /// Gets or sets the lcid.
        /// </summary>
        /// <value>
        /// The lcid.
        /// </value>
        [JsonProperty("lcid")]
        public string Lcid { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        [JsonProperty("alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>
        /// The icon.
        /// </value>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is supported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is supported; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("isSupported")]
        public bool IsSupported { get; set; }
    }
}