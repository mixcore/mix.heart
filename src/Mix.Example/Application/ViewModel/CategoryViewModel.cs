using System;
using System.Threading.Tasks;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.Repository;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel
{
public class CategoryViewModel : ViewModelBase<MixDbContext, CategoryEntity, Guid>
{
    public CategoryViewModel(CategoryEntity entity) : base(entity)
    {
    }

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

    protected override async Task SaveEntityRelationshipAsync(CategoryEntity parentEntity)
    {
        Product.CategoryId = parentEntity.Id;
        await Product.SaveAsync(_unitOfWorkInfo);
    }
}
}
