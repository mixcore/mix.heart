using Microsoft.EntityFrameworkCore;
using Mix.Heart.Enums;
using Mix.Heart.Exceptions;
using Mix.Heart.UnitOfWork;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mix.Heart.Repository
{
    public abstract class RepositoryBase<TDbContext> : IRepositoryBase<TDbContext>, IDisposable
        where TDbContext : DbContext
    {
        public UnitOfWorkInfo UowInfo { get; set; }

        public virtual TDbContext Context { get => (TDbContext)UowInfo?.ActiveDbContext; }

        private bool _isRoot;

        public RepositoryBase()
        {
        }

        protected RepositoryBase(TDbContext dbContext)
        {
            UowInfo = new UnitOfWorkInfo(dbContext);
            _isRoot = true;
        }
        public RepositoryBase(UnitOfWorkInfo unitOfWorkInfo)
        {
            SetUowInfo(unitOfWorkInfo);
        }

        public virtual void SetUowInfo(UnitOfWorkInfo unitOfWorkInfo)
        {
            if (unitOfWorkInfo != null)
            {
                _isRoot = false;
                UowInfo = unitOfWorkInfo;
            };
        }

        protected virtual void BeginUow()
        {
            if (UowInfo == null)
            {
                InitRootUow();
            }
            UowInfo.Begin();

        }

        private void InitRootUow()
        {
            UowInfo ??= new UnitOfWorkInfo(InitDbContext());
            _isRoot = true;
        }

        protected virtual async Task CloseUowAsync()
        {
            if (_isRoot)
            {
                await UowInfo.CloseAsync();
            }
        }

        protected virtual async Task CompleteUowAsync(CancellationToken cancellationToken = default)
        {
            if (_isRoot)
            {
                await UowInfo.CompleteAsync(cancellationToken);
                await UowInfo.CloseAsync();
                return;
            };

            _isRoot = false;
        }

        private TDbContext InitDbContext()
        {
            var dbContextType = typeof(TDbContext);
            var contextCtorInfo = dbContextType.GetConstructor(new Type[] { });

            if (contextCtorInfo == null)
            {
                HandleExceptionAsync(new MixException(MixErrorStatus.ServerError, $"{dbContextType}: Contructor Parameterless Notfound"));
            }

            return (TDbContext)contextCtorInfo.Invoke([]);
        }

        public Task HandleExceptionAsync(Exception ex)
        {
            throw new MixException(MixErrorStatus.Badrequest, ex);
        }

        public void HandleException(
            Exception ex,
            [CallerMemberName] string caller = null, // You must make these optional
            [CallerFilePath] string filePath = null, // by providing a default value.
            [CallerLineNumber] int lineNumber = 0)
        {
            Console.WriteLine("Exception Details:");
            Console.WriteLine(caller);
            Console.WriteLine(filePath);
            Console.WriteLine(lineNumber);
            throw new MixException(MixErrorStatus.Badrequest, ex);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
