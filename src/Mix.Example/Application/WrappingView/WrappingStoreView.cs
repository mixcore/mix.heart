using Mix.Example.Application.ViewModel;
using Mix.Example.Infrastructure;
using Mix.Heart.UnitOfWork;
using Mix.Heart.ViewModel;

namespace Mix.Example.Application.WrappingView
{
    public class WrappingStoreView : WrappingViewBase<MixDbContext>
    {
        public StoreViewModel Store { get; set; }

        public CategoryViewModel Category { get; set; }

        protected override void SaveGroupView(UnitOfWorkInfo uowInfo)
        {
            Store.Save(false, uowInfo);
            Category.Save(false, uowInfo);
        }
    }
}
