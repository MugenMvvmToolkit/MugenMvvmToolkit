using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class CollectionBatchUpdateManagerTest : UnitTestBase
    {
        public CollectionBatchUpdateManagerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BeginEndBatchUpdateShouldNotifyListeners(int listenersCount)
        {
            var batchType = BatchUpdateType.Source;
            var begin = 0;
            var end = 0;
            var collection = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionBatchUpdateListener
                {
                    ThrowErrorNullDelegate = true,
                    OnBeginBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        begin += t == batchType ? 1 : 0;
                    },
                    OnEndBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        end += t == batchType ? 1 : 0;
                    }
                });
            }

            collection.IsInBatch(batchType).ShouldBeFalse();
            var beginBatchUpdate1 = collection.BatchUpdate(batchType);
            collection.IsInBatch(batchType).ShouldBeTrue();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            var beginBatchUpdate2 = collection.BatchUpdate(batchType);
            collection.IsInBatch(batchType).ShouldBeTrue();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            beginBatchUpdate1.Dispose();
            collection.IsInBatch(batchType).ShouldBeTrue();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            beginBatchUpdate2.Dispose();
            collection.IsInBatch(batchType).ShouldBeFalse();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldNotifyListenersOnAdd(int listenersCount)
        {
            var batchType = BatchUpdateType.Source;
            var begin = 0;
            var end = 0;
            var collection = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            var beginBatchUpdate1 = collection.BatchUpdate(batchType);

            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionBatchUpdateListener
                {
                    ThrowErrorNullDelegate = true,
                    OnBeginBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        begin += t == batchType ? 1 : 0;
                    },
                    OnEndBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        end += t == batchType ? 1 : 0;
                    }
                });
            }

            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            beginBatchUpdate1.Dispose();
            collection.IsInBatch(batchType).ShouldBeFalse();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(listenersCount);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldNotifyListenersOnDetach(int listenersCount)
        {
            var batchType = BatchUpdateType.Source;
            var begin = 0;
            var end = 0;
            var collection = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionBatchUpdateListener
                {
                    ThrowErrorNullDelegate = true,
                    OnBeginBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        begin += t == batchType ? 1 : 0;
                    },
                    OnEndBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        end += t == batchType ? 1 : 0;
                    }
                });
            }

            collection.BatchUpdate(batchType);
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            collection.RemoveComponents<ICollectionBatchUpdateManagerComponent>();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(listenersCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldNotifyListenersOnRemove(int listenersCount)
        {
            var batchType = BatchUpdateType.Source;
            var begin = 0;
            var end = 0;
            var collection = new SynchronizedObservableCollection<TestCollectionItem>(ComponentCollectionManager);
            for (var i = 0; i < listenersCount; i++)
            {
                collection.AddComponent(new TestCollectionBatchUpdateListener
                {
                    ThrowErrorNullDelegate = true,
                    OnBeginBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        begin += t == batchType ? 1 : 0;
                    },
                    OnEndBatchUpdate = (c, t) =>
                    {
                        c.ShouldEqual((object) collection);
                        end += t == batchType ? 1 : 0;
                    }
                });
            }

            collection.BatchUpdate(batchType);
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(0);

            collection.RemoveComponents<TestCollectionBatchUpdateListener>();
            begin.ShouldEqual(listenersCount);
            end.ShouldEqual(listenersCount);
        }
    }
}