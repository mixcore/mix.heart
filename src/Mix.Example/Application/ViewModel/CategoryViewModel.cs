using System;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.Entities;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel
{
    public class CategoryViewModel : ViewModelBase<Guid, CategoryEntity, MixDbContext>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public ProductViewModel Product { get; set; }

        protected override void SaveEntityRelationship(CategoryEntity parentEntity, UnitOfWorkInfo uowInfo)
        {
            Product.CategoryId = parentEntity.Id;
            Product.Save(true, uowInfo);
        }
    }
}
