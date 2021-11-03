using Mix.Heart.Enums;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.Infrastructure.Interfaces
{
public interface IMixMediator
{
    Task PublishAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null);
    Task ConsumeAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null);
}
}
