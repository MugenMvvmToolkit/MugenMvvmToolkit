using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestComponentOwner<T> : ComponentOwnerBase<T> where T : class
    {
        public TestComponentOwner(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
        }
    }
}