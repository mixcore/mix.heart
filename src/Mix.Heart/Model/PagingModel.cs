using Mix.Heart.Enums;

namespace Mix.Heart.Model
{
    public class PagingModel : IPagingModel
    {
        public string SortBy { get; set; }

        public SortDirection SortDirection { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPage { get; set; }
    }
}
