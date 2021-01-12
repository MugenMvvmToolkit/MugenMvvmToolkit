using System.Collections.Specialized;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class DefaultTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ValuesShouldBeValid()
        {
            Default.Array<string>().ShouldBeEmpty();
            Default.Array(typeof(string)).ShouldBeEmpty();
            Default.Array<short>().ShouldEqual(Default.Array(typeof(short)));
            Default.ReadOnlyDictionary<object, object>().ShouldBeEmpty();
            (Default.NextCounter() + 1).ShouldEqual(Default.NextCounter());

            Default.EmptyPropertyChangedArgs.PropertyName.ShouldEqual("");
            Default.CountPropertyChangedArgs.PropertyName.ShouldEqual("Count");
            Default.IndexerPropertyChangedArgs.PropertyName.ShouldEqual("Item[]");
            Default.ResetCollectionEventArgs.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);
            Default.CompletedTask.IsCompleted.ShouldBeTrue();
        }

        #endregion
    }
}