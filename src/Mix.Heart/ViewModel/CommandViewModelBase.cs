using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mix.Heart.Entity;
using Mix.Heart.Repository;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.ViewModel
{
public class CommandViewModelBase<TDbContext, TEntity, TPrimaryKey>
    : ViewModelBase<TDbContext, TEntity, TPrimaryKey>
      where TPrimaryKey : IComparable
      where TEntity : class, IEntity<TPrimaryKey>
      where TDbContext : DbContext
{
    public CommandRepository<TDbContext, TEntity, TPrimaryKey> _repository {
        get;
        set;
    }

    public CommandViewModelBase(CommandRepository<TDbContext, TEntity, TPrimaryKey> repository)
    {
        _repository = repository;
    }

    public CommandViewModelBase(UnitOfWorkInfo unitOfWorkInfo)
    {
        _unitOfWorkInfo = unitOfWorkInfo;
        _repository.SetUowInfo(_unitOfWorkInfo);
    }

    public void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
    {
        _repository.SetUowInfo(unitOfWorkInfo);
    }

    #region Async


    public async Task<TPrimaryKey> SaveAsync(UnitOfWorkInfo uowInfo = null)
    {
        try
        {
            BeginUow(uowInfo);
            _repository.SetUowInfo(_unitOfWorkInfo);

            var entity = await SaveHandlerAsync();
            return entity.Id;
        }
        catch(Exception ex)
        {
            HandleException(ex);
            CloseUow();
            return default;
        }
        finally
        {
            CompleteUow();
        }
    }

    public virtual async Task<TEntity> SaveHandlerAsync()
    {
        var entity = InitModel();
        var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(TEntity), GetType()).ReverseMap());
        var mapper = new Mapper(config);
        mapper.Map(this, entity);

        await _repository.SaveAsync(entity);
        return entity;
    }

    #endregion
}
}
