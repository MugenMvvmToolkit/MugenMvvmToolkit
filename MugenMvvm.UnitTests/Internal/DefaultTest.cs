using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
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
            new TestEnumerable().SequenceEqual(new[] {1}).ShouldBeTrue();
            Default.ReadOnlyDictionary<object, object>().ShouldBeEmpty();
            (Default.NextCounter() + 1).ShouldEqual(Default.NextCounter());
            Default.GetOrCreatePropertyChangedArgs("").PropertyName.ShouldEqual("");
            Default.GetOrCreatePropertyChangedArgs("1").PropertyName.ShouldEqual("1");

            Default.EmptyPropertyChangedArgs.PropertyName.ShouldEqual("");
            Default.CountPropertyChangedArgs.PropertyName.ShouldEqual("Count");
            Default.IndexerPropertyChangedArgs.PropertyName.ShouldEqual("Item[]");
            Default.ResetCollectionEventArgs.Action.ShouldEqual(NotifyCollectionChangedAction.Reset);

            Default.Metadata.Count.ShouldEqual(0);
            Default.Metadata.Contains(MetadataContextKey.FromKey<object, object>("test")).ShouldBeFalse();
            Default.Metadata.ToArray().ShouldBeEmpty();
            Default.Metadata.TryGet(MetadataContextKey.FromKey<object, object>("test"), out _, null).ShouldBeFalse();
            Default.Disposable.ShouldNotBeNull();
            Default.WeakReference.Target.ShouldBeNull();
            Default.WeakReference.IsAlive.ShouldBeFalse();
            Default.WeakReference.Release();
            Default.CompletedTask.IsCompleted.ShouldBeTrue();
            Default.TrueTask.Result.ShouldEqual(true);
            Default.FalseTask.Result.ShouldEqual(false);
            Default.NavigationProvider.Id.ShouldEqual("");
        }

        #endregion

        #region Nested types

        private sealed class TestEnumerable : IEnumerable<int>
        {
            #region Implementation of interfaces

            public IEnumerator<int> GetEnumerator() => Default.SingleValueEnumerator(1);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion
        }

        #endregion
    }
}