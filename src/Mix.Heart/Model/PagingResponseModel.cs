﻿using System.Collections.Generic;

namespace Mix.Heart.Models
{
    public class PagingResponseModel<T>
    {
        public PagingResponseModel()
        {

        }
        public PagingResponseModel(IEnumerable<T> data, IPagingModel pagingData)
        {
            Items = data;
            PagingData = pagingData;
        }
        public IEnumerable<T> Items { get; set; }
        public IPagingModel PagingData { get; set; }
    }
}
