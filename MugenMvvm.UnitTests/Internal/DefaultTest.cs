using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class DefaultTest : UnitTestBase
    {
        [Fact]
        public void ValuesShouldBeValid()
        {
            Array.Empty<string>().ShouldBeEmpty();
            Default.Array(typeof(string)).ShouldBeEmpty();
            Array.Empty<short>().ShouldEqual(Default.Array(typeof(short)));
            Default.ReadOnlyDictionary<object, object>().ShouldBeEmpty();
            (Default.NextCounter() + 1).ShouldEqual(Default.NextCounter());

            Default.EmptyPropertyChangedArgs.PropertyName.ShouldEqual("");
            Default.CountPropertyChangedArgs.PropertyName.ShouldEqual("Count");
            Default.IndexerPropertyChangedArgs.PropertyName.ShouldEqual("Item[]");
            Default.ResetCollectionEventArgs.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);
            Task.CompletedTask.IsCompleted.ShouldBeTrue();
        }
    }
}