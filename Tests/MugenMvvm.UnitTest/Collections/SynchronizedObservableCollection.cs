using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.UnitTest.Collections
{
    public class SynchronizedObservableCollection : ObservableCollectionTestBase
    {
        #region Methods

        protected override IObservableCollection<T> CreateCollection<T>(params T[] items)
        {
            return new SynchronizedObservableCollection<T>(items);
        }

        #endregion
    }
}