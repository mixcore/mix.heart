using Microsoft.EntityFrameworkCore;
using Mix.Heart.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
public abstract class RepositoryBase<TDbContext> : IRepositoryBase<TDbContext> where TDbContext : DbContext
{
    public UnitOfWorkInfo _unitOfWorkInfo {
        get;
        set;
    }

    public virtual TDbContext Context => (TDbContext)(_unitOfWorkInfo?.ActiveDbContext);

    private bool _isRoot;

    public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
    {
        _unitOfWorkInfo = unitOfWorkInfo;
    }

    protected RepositoryBase(TDbContext dbContext)
    {
    }

    public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
    {
        if (unitOfWorkInfo != null)
        {
            _isRoot = false;
            _unitOfWorkInfo = unitOfWorkInfo;
        };
    }

    protected virtual void BeginUow(UnitOfWorkInfo uowInfo = null)
    {
        _unitOfWorkInfo ??= uowInfo;
        if (_unitOfWorkInfo != null)
        {
            _isRoot = false;
            if (_unitOfWorkInfo.ActiveTransaction == null)
            {

                _unitOfWorkInfo.SetTransaction(
                    _unitOfWorkInfo.ActiveDbContext.Database.CurrentTransaction
                    ?? _unitOfWorkInfo.ActiveDbContext.Database.BeginTransaction());
            }
            return;
        };

        InitRootUow();

    }

    private void InitRootUow()
    {
        _isRoot = true;

        var dbContext = InitDbContext();

        var dbContextTransaction = dbContext.Database.BeginTransaction();

        _unitOfWorkInfo = new UnitOfWorkInfo();
        _unitOfWorkInfo.SetDbContext(dbContext);
        _unitOfWorkInfo.SetTransaction(dbContextTransaction);
    }

    protected virtual void CompleteUow()
    {
        if (!_isRoot)
        {
            return;
        };

        _unitOfWorkInfo.Complete();

        _isRoot = false;

        Console.WriteLine("Unit of work completed.");
    }

    protected virtual void CloseUow()
    {
        _unitOfWorkInfo.Close();
    }

    protected virtual async Task CompleteUowAsync()
    {
        if (!_isRoot)
        {
            return;
        };

        await _unitOfWorkInfo.CompleteAsync();
        _unitOfWorkInfo.Close();

        _isRoot = false;
    }

    private TDbContext InitDbContext()
    {
        var dbContextType = typeof(TDbContext);
        var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

        if (contextCtorInfo == null)
        {
            throw new NullReferenceException();
        }

        return (TDbContext)contextCtorInfo.Invoke(new object[] { });
    }

    protected void HandleException(Exception ex)
    {
        Console.WriteLine(ex);
        if (_isRoot)
        {
            CloseUow();
        }
        return;
    }


}
}
