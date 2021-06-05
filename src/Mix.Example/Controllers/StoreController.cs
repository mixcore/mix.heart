using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;
using Mix.Example.Application.WrappingView;

namespace Mix.Example.Controllers
{
    [ApiController]
    [Route("stores")]
    public class StoreController : ControllerBase
    {
        [HttpPost("wrap")]
        public void WrapSync([FromBody] WrappingStoreView wrappingView)
        {
            wrappingView.Execute();
        }

        [HttpPost("save")]
        public void SaveSync([FromBody] StoreViewModel storeVm)
        {
            storeVm.Save();
        }
    }
}
