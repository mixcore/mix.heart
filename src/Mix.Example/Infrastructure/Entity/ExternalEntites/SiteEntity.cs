using Mix.Heart.Entities;

namespace Mix.Example.Infrastructure.ExternalEntites {
  public class SiteEntity : Entity<int> {
    public string Name { get; set; }

    public string Description { get; set; }
  }
}
