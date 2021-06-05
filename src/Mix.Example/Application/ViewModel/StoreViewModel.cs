﻿using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.ViewModel;
using System;

namespace Mix.Example.Application.ViewModel
{
    public class StoreViewModel : ViewModelBase<Guid, StoreEntity, MixDbContext>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string Country { get; set; }
    }
}
