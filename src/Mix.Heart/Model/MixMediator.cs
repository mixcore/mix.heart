using Mix.Heart.Enums;
using Mix.Heart.Infrastructure.Interfaces;
using System;
using System.Threading.Tasks;

namespace Mix.Heart.Model
{
    public class MixMediator : IMixMediator
    {
        public Task ConsumeAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null)
        {
            Console.WriteLine(sender);
            Console.WriteLine(action);
            Console.WriteLine(isSucceed);
            Console.WriteLine(ex);
            return Task.CompletedTask;
        }

        public Task PublishAsync(object sender, MixViewModelAction action, bool isSucceed, Exception ex = null)
        {
            return Task.CompletedTask;
        }
    }
}
