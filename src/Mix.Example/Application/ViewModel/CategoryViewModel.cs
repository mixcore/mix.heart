using System;
using System.Threading;
using System.Threading.Tasks;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel {
  public class CategoryViewModel
      : ViewModelBase<MixDbContext, CategoryEntity, Guid, CategoryViewModel> {
    public CategoryViewModel(CategoryEntity entity) : base(entity) {}

    public string Name { get; set; }

    public string Description { get; set; }

    public ProductViewModel Product { get; set; }

    protected override Task
    SaveEntityRelationshipAsync(CategoryEntity parentEntity,
                                CancellationToken cancellationToken = default) {
      return Task.CompletedTask;
    }
  }
}
