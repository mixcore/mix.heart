using Mix.Heart.Enums;
using Newtonsoft.Json;
using System;

namespace Mix.Heart.Models
{
    /// <summary>
    ///
    /// </summary>
    public class RequestPaging
    {
        /// <summary>
        /// Gets or sets the view type.
        /// </summary>
        /// <value>
        /// The view Type.
        /// </value>
        [JsonProperty("viewType")]
        public string ViewType { get; set; }

        /// <summary>
        /// Gets or sets the country identifier.
        /// </summary>
        /// <value>
        /// The country identifier.
        /// </value>
        [JsonProperty("countryId")]
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        [JsonProperty("culture")]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        [JsonProperty("direction")]
        public DisplayDirection Direction { get; set; } = DisplayDirection.Asc;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the keyword.
        /// </summary>
        /// <value>
        /// The keyword.
        /// </value>
        [JsonProperty("keyword")]
        public string Keyword { get; set; }

        /// <summary>
        /// Gets or sets the keyword.
        /// </summary>
        /// <value>
        /// The keyword.
        /// </value>
        [JsonProperty("query")]
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>
        /// The order by.
        /// </value>
        [JsonProperty("orderBy")]
        public string OrderBy { get; set; } = "Id";

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        [JsonProperty("skip")]
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the topof the page.
        /// </summary>
        /// <value>
        /// The top of the page.
        /// </value>
        [JsonProperty("top")]
        public int? Top { get; set; }

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        /// <value>
        /// The user agent.
        /// </value>
        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("fromDate")]
        public DateTime? FromDate { get; set; }

        [JsonProperty("toDate")]
        public DateTime? ToDate { get; set; }
    }
}
