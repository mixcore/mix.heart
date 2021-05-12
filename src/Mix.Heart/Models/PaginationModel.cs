using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mix.Heart.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginationModel<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationModel{T}"/> class.
        /// </summary>
        public PaginationModel()
        {
            PageIndex = 0;
            PageSize = 0;
            TotalItems = 0;
            TotalPage = 1;
            Items = new List<T>();
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        [JsonProperty("items")]
        public List<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// The index of the page.
        /// </value>
        [JsonProperty("page")]
        public int Page { get { return PageIndex + 1; } }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>
        /// The size of the page.
        /// </value>
        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total items.
        /// </summary>
        /// <value>
        /// The total items.
        /// </value>
        [JsonProperty("totalItems")]
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the total page.
        /// </summary>
        /// <value>
        /// The total page.
        /// </value>
        [JsonProperty("totalPage")]
        public int TotalPage { get; set; }
    }
}
