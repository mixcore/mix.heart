using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Mix.Example.Dto;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.Repository;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel
{
public class ProductViewModel : ViewModelBase<MixDbContext, ProductEntity, Guid>
{
    public ProductViewModel(CommandRepository<MixDbContext, ProductEntity, Guid> repository) : base(repository)
    {
    }

    public ProductViewModel(SaveProductDto dto) : base(dto)
    {
        MappDto(dto);
    }

    public string Name {
        get;
        set;
    }

    public string Description {
        get;
        set;
    }

    public string Producer {
        get;
        set;
    }

    /// <summary>
    /// TODO: consider with JsonIgnore attribute
    /// </summary>
    [JsonIgnore]
    public Guid CategoryId {
        get;
        set;
    }

    public List<ProductDetailViewModel> ProductDetails {
        get;
        set;
    } = new List<ProductDetailViewModel>();

    protected void MappDto(SaveProductDto dto)
    {
        foreach (var item in dto.ProductDetailDtos)
        {
            ProductDetails.Add(new ProductDetailViewModel(item));
        }
    }

    protected override async Task SaveEntityRelationshipAsync(ProductEntity parentEntity)
    {
        // TODO: Save view list need to improve
        foreach (var detail in ProductDetails)
        {
            detail.ProductId = parentEntity.Id;
            await detail.SaveAsync(_unitOfWorkInfo);
        }
    }
}
}
