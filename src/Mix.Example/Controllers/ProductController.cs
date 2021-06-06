using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mix.Example.Application.ViewModel;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.Repository;

namespace Mix.Example.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductController : ControllerBase
    {
        private readonly CommandRepository<MixDbContext, ProductEntity, Guid> repository;

        public ProductController(CommandRepository<MixDbContext, ProductEntity, Guid> repository)
        {
            this.repository = repository;
        }

        [HttpPost("save")]
        public int SaveSync([FromBody] ProductViewModel productVm)
        {
            productVm.Save();

            return 1;
        }

        [HttpPost("save-async")]
        public Task<Guid> SaveASync()
        {
            var productVm = new ProductViewModel(repository).SaveAsync();

            return productVm;
        }
    }
}
