﻿using Mix.Heart.Enums;

namespace Mix.Heart.Models
{
    public class PagingModel
    {
        public int PageIndex { get; set; }

        public int? PageSize { get; set; }

        public int Total { get; set; }

        public int TotalPage { get; set; }

        public string SortBy { get; set; }

        public SortDirection SortDirection { get; set; }
    }
}