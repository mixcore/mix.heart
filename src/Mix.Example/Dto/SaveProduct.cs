using Mix.Example.Application.ViewModel;

namespace Mix.Example.Dto
{
    public class SaveProduct
    {
        public bool WithNewCategory { get; set; }

        public CategoryViewModel Category { get; set; }

        public ProductViewModel Product { get; set; }
    }
}
