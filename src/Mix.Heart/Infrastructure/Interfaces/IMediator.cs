using Mix.Heart.Enums;
using System.Threading.Tasks;

namespace Mix.Heart.Infrastructure.Interfaces
{
    public interface IMediator
    {
        void Notify(object sender, RepositoryAction action, bool isSucceed);
        Task NotifyAsync(object sender, RepositoryAction action, bool isSucceed);
    }
}
