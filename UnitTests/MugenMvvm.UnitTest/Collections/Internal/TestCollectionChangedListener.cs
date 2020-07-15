using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.UnitTest.Collections.Internal
{
    public class TestCollectionChangedListener<T> : TestCollectionChangedListenerBase<T>, ICollectionChangedListener
    {
        #region Constructors

        public TestCollectionChangedListener(IObservableCollection<T> collection) : base(collection)
        {
        }

        #endregion
    }
}