using System;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Collections;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    [Collection(SharedContext)]
    public class CollectionSynchronizerTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _source;
        private readonly SynchronizedObservableCollection<object> _target;

        public CollectionSynchronizerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _source = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _target = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void ShouldSynchronizeBatchUpdates()
        {
            var batchCount = 0;
            var token = _target.SynchronizeWith(_source);
            _target.AddComponent(new TestCollectionBatchUpdateListener
            {
                OnBeginBatchUpdate = (_, type) =>
                {
                    type.ShouldEqual(BatchUpdateType.Source);
                    ++batchCount;
                },
                OnEndBatchUpdate = (_, type) =>
                {
                    type.ShouldEqual(BatchUpdateType.Source);
                    --batchCount;
                }
            });

            var b1 = _source.BatchUpdate();
            batchCount.ShouldEqual(1);

            var b2 = _source.BatchUpdate();
            batchCount.ShouldEqual(1);

            b1.Dispose();
            batchCount.ShouldEqual(1);

            b2.Dispose();
            batchCount.ShouldEqual(0);

            b1 = _source.BatchUpdate();
            b2 = _source.BatchUpdate();
            batchCount.ShouldEqual(1);
            token.Dispose();
            batchCount.ShouldEqual(0);

            b1.Dispose();
            b2.Dispose();
            batchCount.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSynchronizeItems()
        {
            var token = _target.SynchronizeWith(_source);
            for (var i = 0; i < 2; i++)
            {
                _source.Add(1);
                Assert(i == 0);

                _source.Insert(1, 2);
                Assert(i == 0);

                _source.Move(0, 1);
                Assert(i == 0);

                _source.Remove(2);
                Assert(i == 0);

                _source.RemoveAt(0);
                Assert(i == 0);

                _source.Reset(new object[] {1, 2, 3, 4, 5});
                Assert(i == 0);

                _source[0] = 200;
                Assert(i == 0);

                _source.Clear();
                Assert(i == 0);

                token.Dispose();
            }
        }
        
        [Fact(Skip = ReleaseTest)]
        public void ShouldBeWeak()
        {
            var weakReference = WeakTest(_source);
            GcCollect();
            GcCollect();
            GcCollect();
            _source.Add(NewId());
            weakReference.IsAlive.ShouldBeFalse();
        }

        private WeakReference WeakTest(SynchronizedObservableCollection<object> target)
        {
            var collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            collection.SynchronizeWith(target);

            target.Add(NewId());
            collection.ShouldEqual(target);
            return new WeakReference(collection);
        }

        private void Assert(bool hasListener)
        {
            if (hasListener)
                _source.ShouldEqual(_target);
            else
                _target.ShouldBeEmpty();
        }
    }
}