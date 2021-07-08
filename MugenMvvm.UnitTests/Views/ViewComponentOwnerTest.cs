using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.Views;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewComponentOwnerTest : ComponentOwnerTestBase<IView>
    {
        protected override IView GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) =>
            new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), DefaultMetadata), this, new TestViewModel(), null, componentCollectionManager);
    }
}