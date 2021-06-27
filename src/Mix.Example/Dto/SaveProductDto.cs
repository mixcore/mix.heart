using Mix.Example.Infrastructure.MixEntities;
using System.Collections.Generic;

namespace Mix.Example.Dto
{
public class SaveProductDto: ProductEntity
{
    public List<SaveProductDetailDto> ProductDetailDtos {
        get;
        set;
    }
}
}
