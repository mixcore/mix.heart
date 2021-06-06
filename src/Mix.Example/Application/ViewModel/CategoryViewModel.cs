using System;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel {
  public class CategoryViewModel
      : ViewModelBase<MixDbContext, CategoryEntity, Guid> {
    public string Name {
      get;
      set;
    }

    public string Description {
      get;
      set;
    }

    public ProductViewModel Product {
      get;
      set;
    }

    protected override void
    SaveEntityRelationship(CategoryEntity parentEntity) {
      Product.CategoryId = parentEntity.Id;
      Product.Save(false, _unitOfWorkInfo);
    }
  }
}
