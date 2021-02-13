// Licensed to the Mixcore Foundation under one or more agreements.
// The Mixcore Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Mix.Heart.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mix.Domain.Core.ViewModels
{
    /// <summary>
    /// Api Result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        [JsonProperty("data")]
        public T Data { get; set; }

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
    }

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

    public class RequestEncrypted
    {
        [JsonProperty("encrypted")]
        public string Encrypted { get; set; }

        [JsonProperty("plainText")]
        public string PlainText { get; set; }

        [JsonProperty("iv")]
        public string IV { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }

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
        public MixHeartEnums.DisplayDirection Direction { get; set; } = MixHeartEnums.DisplayDirection.Asc;

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