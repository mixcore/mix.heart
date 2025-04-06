using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entities;
using Mix.Heart.Enums;
using Mix.Heart.UnitOfWork;
using System;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView> : SimpleViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
        where TView : ViewModelBase<TDbContext, TEntity, TPrimaryKey, TView>
    {
        #region Properties

        public DateTime CreatedDateTime { get; set; }

        public DateTime? LastModified { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public int Priority { get; set; }

        public MixContentStatus Status { get; set; } = MixContentStatus.Published;

        public bool IsDeleted { get; set; }

        #endregion

        #region Constructors

        public ViewModelBase() : base()
        {
        }

        public ViewModelBase(TDbContext context) : base(context)
        {
        }

        public ViewModelBase(TEntity entity, UnitOfWorkInfo uowInfo) : base(entity, uowInfo)
        {
        }

        public ViewModelBase(UnitOfWorkInfo unitOfWorkInfo) : base(unitOfWorkInfo)
        {
        }

        #endregion

        #region Overrides

        public override void InitDefaultValues(string language = null, int? cultureId = null)
        {
            CreatedDateTime = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
            Status = MixContentStatus.Published;
            IsDeleted = false;
        }

        #endregion
    }
}
