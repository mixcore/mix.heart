using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;

namespace Mix.Example.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        [HttpPost("save")]
        public int SaveSync([FromBody] CategoryViewModel categoryDto)
        {
            categoryDto.Save(true);

            return 1;
        }
    }
}
