using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;

namespace Mix.Example.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        public ProductController()
        {
        }

        [HttpPost("save")]
        public int SaveSync([FromBody] ProductViewModel productVm)
        {
            productVm.Save();

            return 1;
        }

        [HttpPost("save-async")]
        public Task<int> SaveASync()
        {
            var productVm = new ProductViewModel().SaveAsync();

            return productVm;
        }
    }
}
