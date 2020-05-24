using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewComponentOwnerTest : ComponentOwnerTestBase<IView>
    {
        #region Methods

        protected override IView GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, collectionProvider);
        }

        #endregion
    }
}