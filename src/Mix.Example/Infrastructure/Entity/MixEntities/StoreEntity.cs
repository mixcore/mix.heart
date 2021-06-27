using Mix.Heart.Entities;

namespace Mix.Example.Infrastructure.MixEntities {
  public class StoreEntity : Entity {
    public string Name { get; set; }

    public string Description { get; set; }

    public string Address { get; set; }

    public string Country { get; set; }
  }
}
