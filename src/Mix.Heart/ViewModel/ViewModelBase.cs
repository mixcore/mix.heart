using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using Mix.Heart.Enums;
using Mix.Heart.UnitOfWork;
using System;

namespace Mix.Heart.ViewModel
{
    public abstract partial class ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        : IViewModel<TPrimaryKey>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public UnitOfWorkInfo _unitOfWorkInfo { get; set; }
        public TPrimaryKey Id { get; set; }
        public DateTime CreatedDateTime { get; set;}
        public DateTime? LastModified { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public int Priority { get; set; }
        public MixContentStatus Status { get; set;}

        protected void HandleException(Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }
    }
}
