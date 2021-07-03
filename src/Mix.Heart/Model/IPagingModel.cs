using Mix.Heart.Enums;

namespace Mix.Heart.Model
{
    public interface IPagingModel
    {
        int PageIndex { get; set; }

        int PageSize { get; set; }

        int Total { get; set; }

        int TotalPage { get; set; }

        string SortBy { get; set; }

        SortDirection SortDirection { get; set; }
    }
}
