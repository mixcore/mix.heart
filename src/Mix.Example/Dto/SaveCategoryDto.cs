using Mix.Example.Infrastructure.MixEntities;

namespace Mix.Example.Dto {
  public class SaveCategoryDto : CategoryEntity {
    public SaveProductDto Product { get; set; }
  }
}
