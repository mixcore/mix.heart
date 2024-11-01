using Mix.Heart.Enums;
using Mix.Heart.Model;
using System.Collections.Generic;

namespace Mix.Heart.Models
{
    public class PagingModel
    {
        public int Page { get; set; }
        public int PageIndex { get; set; }
        public string? PagingState { get; set; } // use for Cassandra only
        public int? PageSize { get; set; }
        public string SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
        public long Total { get; set; }

        public int TotalPage { get; set; }

        public List<MixSortByField> SortByFields { get; set; } = new List<MixSortByField>();
    }
}
