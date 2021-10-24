using Mix.Heart.Entities.Cache;
using Mix.Heart.Enums;
using Mix.Heart.UnitOfWork;
using System;

namespace Mix.Heart.ViewModel
{
    public class MixCacheViewModel : ViewModelBase<MixCacheDbContext, MixCache, string, MixCacheViewModel>
    {
        public string Value { get; set; }
        public DateTime? ExpiredDateTime { get; set; }
        public new MixCacheStatus Status { get; set; }

        public MixCacheViewModel()
        {
        }

        public MixCacheViewModel(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }

        public MixCacheViewModel(MixCache entity, UnitOfWorkInfo uowInfo = null) : base(entity, uowInfo)
        {
        }
    }
}
