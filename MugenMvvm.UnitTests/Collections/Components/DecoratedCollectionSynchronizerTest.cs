using MugenMvvm.Collections;
using MugenMvvm.Collections.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Tests.Collections;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public class DecoratedCollectionSynchronizerTest : UnitTestBase
    {
        private readonly SynchronizedObservableCollection<object> _source;
        private readonly SynchronizedObservableCollection<object?> _target;

        public DecoratedCollectionSynchronizerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _source = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            _source.AddComponent(new ImmutableItemConverterCollectionDecorator<object, string>(o => "Item " + o.GetHashCode()));
            _target = new SynchronizedObservableCollection<object?>(ComponentCollectionManager);
        }

        [Fact]
        public void ShouldSynchronizeBatchUpdates()
        {
            var batchCount = 0;
            var token = _target.SynchronizeDecoratedItemsWith(_source);
            var manager = _source.GetComponent<ICollectionDecoratorManagerComponent>();
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

            var b1 = manager.BatchUpdate(_source);
            batchCount.ShouldEqual(1);

            var b2 = manager.BatchUpdate(_source);
            batchCount.ShouldEqual(1);

            b1.Dispose();
            batchCount.ShouldEqual(1);

            b2.Dispose();
            batchCount.ShouldEqual(0);

            b1 = manager.BatchUpdate(_source);
            b2 = manager.BatchUpdate(_source);
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
            var token = _target.SynchronizeDecoratedItemsWith(_source);
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

        private void Assert(bool hasListener)
        {
            if (hasListener)
                _source.DecoratedItems().ShouldEqual(_target);
            else
                _target.ShouldBeEmpty();
        }
    }
}