using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
    public class CommandViewModelBase<TDbContext, TEntity, TPrimaryKey>
        : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
        where TPrimaryKey : IComparable
        where TEntity : class, IEntity<TPrimaryKey>
        where TDbContext : DbContext
    {
        public CommandRepository<TDbContext, TEntity, TPrimaryKey> _repository { get; set; }

        public CommandViewModelBase(CommandRepository<TDbContext, TEntity, TPrimaryKey> repo)
        {
            UnitOfWorkInfo = new UnitOfWorkInfo();
            _repository = repo;
            
        }

        public void SetUow(UnitOfWorkInfo unitOfWorkInfo)
        {
            _repository.SetUow(unitOfWorkInfo);
        }

        #region Async
        public virtual TEntity InitModel()
        {
            Type classType = typeof(TEntity);
            return (TEntity)Activator.CreateInstance(classType);
        }

        public virtual async Task<TPrimaryKey> SaveAsync()
        {
            var entity = InitModel();
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TEntity), GetType()).ReverseMap());
            var mapper = new Mapper(config);
            mapper.Map(this , entity);
            await _repository.SaveAsync(entity);
            return entity.Id;
        }

        #endregion
    }
}
