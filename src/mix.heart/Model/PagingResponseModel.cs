using System.Collections.Generic;

namespace Mix.Heart.Models
{
    public interface IPagingResponse { }

    public interface IPagingResponse<T> : IPagingResponse
    {
        public IEnumerable<T> Items { get; set; }

        public PagingModel PagingData { get; set; }
    }

    public class PagingResponseModel<T> : IPagingResponse<T>
    {
        public PagingResponseModel()
        {
        }

        public PagingResponseModel(IEnumerable<T> data, PagingModel pagingData)
        {
            Items = data;
            PagingData = pagingData;
        }

        public IEnumerable<T> Items { get; set; }

        public PagingModel PagingData { get; set; }
    }
}
