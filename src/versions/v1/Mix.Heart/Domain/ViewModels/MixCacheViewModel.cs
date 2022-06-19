using Microsoft.EntityFrameworkCore.Storage;
using Mix.Domain.Data.ViewModels;
using Mix.Heart.Domain.Entities;
using Mix.Heart.Domain.Enums;
using System;
using System.Data;

namespace Mix.Heart.Domain.ViewModels
{
    public class MixCacheViewModel: ViewModelBase<MixCacheDbContext, MixCache, MixCacheViewModel>
    {
        #region Properties
        public string Id { get; set; }
        public string Value { get; set; }
        public DateTime? ExpiredDateTime { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int Priority { get; set; }
        public MixCacheStatus Status { get; set; }
        #endregion

        #region Constructors

        public MixCacheViewModel(): base()
        {
            IsCache = false;
            Repository.IsCache = false;
        }

        public MixCacheViewModel(MixCache model, MixCacheDbContext context, IDbContextTransaction transaction ): base(model, context, transaction)
        {

        }
        #endregion

        #region Overrides
        
        #endregion

    }
}
