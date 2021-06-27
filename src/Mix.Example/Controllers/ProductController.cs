using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;
using Mix.Example.Dto;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.Repository;

namespace Mix.Example.Controllers {
  [ApiController]
  [Route("products")]
  public class ProductController : ControllerBase {
    private readonly CommandRepository<MixDbContext, ProductEntity, Guid>
        repository;

    public ProductController(
        CommandRepository<MixDbContext, ProductEntity, Guid> repository) {
      this.repository = repository;
    }

    [HttpPost("save")]
    public async Task<int> SaveSyncAsync([FromBody] SaveProductDto dto) {
      var productVm = new ProductViewModel(dto);
      await productVm.SaveAsync();

      return 1;
    }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<Guid>> Delete(Guid id) {
      await repository.DeleteAsync(id);
      return Ok(id);
    }
  }
}
