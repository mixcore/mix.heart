using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;
using Mix.Example.Dto;
using System.Threading.Tasks;

namespace Mix.Example.Controllers {
  [ApiController]
  [Route("categories")]
  public class CategoryController : ControllerBase {
    [HttpPost("save")]
    public async Task<int>
    SaveSyncAsync([FromBody] SaveCategoryDto categoryDto) {
      var cate = new CategoryViewModel(categoryDto);
      await cate.SaveAsync();
      return 1;
    }
  }
}
