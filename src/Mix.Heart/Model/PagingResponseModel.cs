using Mix.Heart.Model;
using System.Collections.Generic;

namespace Mix.Heart.Model
{
    public class PagingResponseModel<T>
    {
        public PagingResponseModel(IEnumerable<T> data, IPagingModel pagingData)
        {
            Items = data;
            PagingData = pagingData;
        }
        public IEnumerable<T> Items { get; set; }
        public IPagingModel PagingData { get; set; }
    }
}
