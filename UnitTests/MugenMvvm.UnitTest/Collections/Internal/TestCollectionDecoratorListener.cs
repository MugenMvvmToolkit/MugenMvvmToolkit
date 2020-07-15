using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionDecoratorListener<T> : TestCollectionChangedListenerBase<T>, ICollectionDecoratorListener
    {
        #region Constructors

        public TestCollectionDecoratorListener(IObservableCollection<T> collection) : base(collection)
        {
        }

        #endregion
    }
}