using System;
using System.Text.Json.Serialization;
using Mix.Example.Infrastructure;
using Mix.Example.Infrastructure.MixEntities;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.ViewModel
{
public class ProductDetailViewModel : ViewModelBase<MixDbContext, ProductDetailEntity, Guid>
{
    public string Name {
        get;
        set;
    }

    public string Description {
        get;
        set;
    }

    public int Quantity {
        get;
        set;
    }

    public int InventoryNumber {
        get;
        set;
    }

    /// <summary>
    /// TODO: consider with JsonIgnore attribute
    /// </summary>
    [JsonIgnore]
    public Guid ProductId {
        get;
        set;
    }
}
}
