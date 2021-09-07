using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Extensions
{
    [Category(SharedContext)]
    public class CollectionExtensionsTest : UnitTestBase
    {
        private readonly Random _random;
        private readonly SynchronizedObservableCollection<object> _collection;

        public CollectionExtensionsTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _random = new Random();
            _collection = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
            RegisterDisposeToken(WithGlobalService(ComponentCollectionManager));
        }

        [Fact]
        public void AutoRefreshOnObservableShouldSubscribeToObservable()
        {
            const int count = 10;
            var changedEvents = new List<object>();
            _collection.AddComponent(new TestDecoratedCollectionChangedListener<object>
            {
                OnChanged = (_, item, _, args) =>
                {
                    args.ShouldEqual(this);
                    changedEvents.Add(item!);
                }
            });

            for (var i = 0; i < count; i++)
                _collection.Add(new TestObservable());

            _collection.ConfigureDecorators().AutoRefreshOnObservable<TestObservable, object>(observable => observable, this);
            for (var i = 0; i < count; i++)
            {
                var testObservable = (TestObservable)_collection[i];
                testObservable.Count.ShouldEqual(1);
                testObservable[0].OnNext(null!);
            }

            changedEvents.Count.ShouldEqual(count);
            changedEvents.ShouldEqualUnordered(_collection);

            changedEvents.Clear();
            var oldItems = _collection.ToArray();
            _collection.Clear();

            for (var i = 0; i < count; i++)
            {
                var changedModel = (TestObservable)oldItems[i];
                changedModel.Count.ShouldEqual(0);
            }
        }

        [Fact]
        public void AutoRefreshOnPropertyChangedShouldListenPropertyChanges()
        {
            const int count = 10;
            var changedEvents = new List<object>();
            _collection.AddComponent(new TestDecoratedCollectionChangedListener<object>
            {
                OnChanged = (_, item, _, args) =>
                {
                    args.ShouldEqual(this);
                    changedEvents.Add(item!);
                }
            });

            for (var i = 0; i < count; i++)
                _collection.Add(new TestNotifyPropertyChangedModel());

            _collection.ConfigureDecorators().AutoRefreshOnPropertyChanged<TestNotifyPropertyChangedModel>(nameof(TestNotifyPropertyChangedModel.Property), this);
            for (var i = 0; i < count; i++)
            {
                ((TestNotifyPropertyChangedModel)_collection[i]).OnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property));
                ((TestNotifyPropertyChangedModel)_collection[i]).OnPropertyChanged("Test");
            }

            changedEvents.Count.ShouldEqual(count);
            changedEvents.ShouldEqualUnordered(_collection);

            changedEvents.Clear();
            var oldItems = _collection.ToArray();
            _collection.Clear();

            for (var i = 0; i < count; i++)
            {
                var changedModel = (TestNotifyPropertyChangedModel)oldItems[i];
                changedModel.HasSubscribers.ShouldBeFalse();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BindToSourceShouldCreateReadOnlyObservableCollection(bool disposeSource)
        {
            var readOnlyObservableCollection = (ReadOnlyObservableCollection<object>)_collection.BindToSource(disposeSource);
            readOnlyObservableCollection.Dispose();
            _collection.IsDisposed.ShouldEqual(disposeSource);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BindShouldCreateReadOnlyObservableCollection(bool disposeSource)
        {
            var readOnlyObservableCollection = (DecoratedReadOnlyObservableCollection<object>)_collection.Bind(disposeSource);
            readOnlyObservableCollection.Dispose();
            _collection.IsDisposed.ShouldEqual(disposeSource);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(2)]
        public void ConfigureDecoratorsShouldUseLatestPriority(int step)
        {
            _collection.ConfigureDecorators(10).Priority.ShouldEqual(10);
            _collection.ConfigureDecorators(step: step).Step.ShouldEqual(step);

            _collection.ConfigureDecorators().Priority.ShouldEqual(0);

            _collection.AddComponent(new TestCollectionDecorator { Priority = 0 });
            _collection.ConfigureDecorators(step: step).Priority.ShouldEqual(-step);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CountShouldBeValid(bool withCondition)
        {
            const int count = 1000;
            var countValue = 0;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -10000;
            _collection.ConfigureDecorators().Count(i => countValue = i, condition);
            if (!withCondition)
                condition = _ => true;
            Action assert = () => countValue.ShouldEqual(_collection.OfType<IntValue>().Count(condition!));

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue { Value = next });

                assert();
            }

            foreach (var value in _collection.OfType<IntValue>())
            {
                value.Value = _random.Next(int.MinValue, int.MaxValue);
                _collection.RaiseItemChanged(value);
                assert();
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue { Value = _random.Next() }));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue)_collection[i];
                o.Value = _random.Next();
                values.Insert(i, o);
            }

            _collection.Reset(values);
            assert();

            values.AddRange(values.ToArray());
            for (var i = 0; i < values.Count / 2; i++)
                values[i].Value = _random.Next();
            _collection.Reset(values);
            assert();

            _collection.Clear();
            assert();
            countValue.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GroupByShouldBeValid(bool flatten)
        {
            var group1Cleanup = 0;
            var group1 = new Group(0, () =>
            {
                ++group1Cleanup;
                return true;
            });
            var group2Cleanup = 0;
            var group2 = new Group(1, () =>
            {
                ++group2Cleanup;
                return false;
            });
            _collection.ConfigureDecorators()
                       .GroupBy<int, Group>(i => i % 2 == 0 ? group1 : group2, SortingComparer<Group>.Descending(group => group.Value).Build(), null, flatten);

            for (var i = 0; i < 10; i++)
                _collection.Add(i);

            group1.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 == 0));
            group2.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 != 0));

            _collection.DecoratedItems().ShouldEqual(flatten ? group2.Items.Concat(group1.Items).OfType<object>() : new[] { group2, group1 });

            _collection.Clear();
            group1Cleanup.ShouldEqual(1);
            group1.Items.ShouldNotBeEmpty();

            group2Cleanup.ShouldEqual(1);
            group2.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MaxShouldBeValid(bool withCondition)
        {
            const int count = 1000;
            var maxValue = int.MinValue;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value < 10000;
            _collection.ConfigureDecorators().Max(i => i.Value, i => maxValue = i, maxValue, condition);
            if (!withCondition)
                condition = _ => true;
            Action assert = () =>
            {
                var ints = _collection.OfType<IntValue>().Where(condition!).Select(value => value.Value);
                maxValue.ShouldEqual(ints.Any() ? ints.Max() : int.MinValue);
            };

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue { Value = next });

                assert();
            }

            foreach (var value in _collection.OfType<IntValue>())
            {
                value.Value = _random.Next(int.MinValue, int.MaxValue);
                _collection.RaiseItemChanged(value);
                assert();
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue { Value = _random.Next() }));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue)_collection[i];
                o.Value = _random.Next();
                values.Insert(i, o);
            }

            _collection.Reset(values);
            assert();

            values.AddRange(values.ToArray());
            for (var i = 0; i < values.Count / 2; i++)
                values[i].Value = _random.Next();
            _collection.Reset(values);
            assert();

            _collection.Clear();
            assert();
            maxValue.ShouldEqual(int.MinValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MinShouldBeValid(bool withCondition)
        {
            const int count = 1000;
            var minValue = int.MaxValue;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -10000;
            _collection.ConfigureDecorators().Min(i => i.Value, i => minValue = i, minValue, condition);
            if (!withCondition)
                condition = _ => true;
            Action assert = () =>
            {
                var ints = _collection.OfType<IntValue>().Where(condition!).Select(value => value.Value);
                minValue.ShouldEqual(ints.Any() ? ints.Min() : int.MaxValue);
            };

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue { Value = next });

                assert();
            }

            foreach (var value in _collection.OfType<IntValue>())
            {
                value.Value = _random.Next(int.MinValue, int.MaxValue);
                _collection.RaiseItemChanged(value);
                assert();
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue { Value = _random.Next() }));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue)_collection[i];
                o.Value = _random.Next();
                values.Insert(i, o);
            }

            _collection.Reset(values);
            assert();

            values.AddRange(values.ToArray());
            for (var i = 0; i < values.Count / 2; i++)
                values[i].Value = _random.Next();
            _collection.Reset(values);
            assert();

            _collection.Clear();
            assert();
            minValue.ShouldEqual(int.MaxValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SumShouldBeValid1(bool withCondition)
        {
            const int count = 1000;
            var sum = 0;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -1000;
            _collection.ConfigureDecorators().Sum(arg => arg.Value, i => sum = i, condition);
            if (!withCondition)
                condition = _ => true;

            int Next()
            {
                return _random.Next(-10000, 10000);
            }

            Action assert = () => sum.ShouldEqual(_collection.OfType<IntValue>().Where(condition!).Sum(value => value.Value));
            for (var i = 1; i < count; i++)
            {
                var next = Next();
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue { Value = next });

                assert();
            }

            foreach (var value in _collection.OfType<IntValue>())
            {
                value.Value = Next();
                _collection.RaiseItemChanged(value);
                assert();
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue { Value = Next() }));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue)_collection[i];
                o.Value = Next();
                values.Insert(i, o);
            }

            _collection.Reset(values);
            assert();

            values.AddRange(values.ToArray());
            for (var i = 0; i < values.Count / 2; i++)
                values[i].Value = Next();
            _collection.Reset(values);
            assert();

            _collection.Clear();
            assert();
            sum.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SumShouldBeValid2(bool withCondition)
        {
            const int count = 1000;
            decimal sum = 0;
            Func<DecimalValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -1000;
            _collection.ConfigureDecorators().Sum(arg => arg.Value, i => sum = i, condition);
            if (!withCondition)
                condition = _ => true;

            decimal Next()
            {
                return (decimal)(_random.NextDouble() * _random.Next(-10000, 10000));
            }

            Action assert = () => sum.ShouldEqual(_collection.OfType<DecimalValue>().Where(condition!).Sum(value => value.Value));
            for (var i = 1; i < count; i++)
            {
                var next = Next();
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new DecimalValue { Value = next });

                assert();
            }

            foreach (var value in _collection.OfType<DecimalValue>())
            {
                value.Value = Next();
                _collection.RaiseItemChanged(value);
                assert();
            }

            var values = new List<DecimalValue>(Enumerable.Range(0, count / 2).Select(_ => new DecimalValue { Value = Next() }));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (DecimalValue)_collection[i];
                o.Value = Next();
                values.Insert(i, o);
            }

            _collection.Reset(values);
            assert();

            values.AddRange(values.ToArray());
            for (var i = 0; i < values.Count / 2; i++)
                values[i].Value = Next();
            _collection.Reset(values);
            assert();

            _collection.Clear();
            assert();
            sum.ShouldEqual(0);
        }

        [Fact]
        public void TrackSelectedItemShouldTrackSelectedItem()
        {
            object? selectedItem = null;
            _collection.ConfigureDecorators().TrackSelectedItem(() => selectedItem, i => selectedItem = i, (ints, i) =>
            {
                if (!ints.Any())
                    return int.MaxValue;
                i.ShouldEqual(selectedItem);
                return ints.Max(j => (int)j);
            });

            _collection.Add(1);
            selectedItem.ShouldEqual(1);

            _collection.Add(2);
            selectedItem.ShouldEqual(1);

            _collection.Remove(1);
            selectedItem.ShouldEqual(2);

            _collection.Add(3);
            _collection.Add(4);
            selectedItem.ShouldEqual(2);

            _collection.Remove(2);
            selectedItem.ShouldEqual(4);

            _collection.Clear();
            selectedItem.ShouldEqual(int.MaxValue);
        }

        private sealed class IntValue
        {
            public int Value { get; set; }
        }

        private sealed class DecimalValue
        {
            public decimal Value { get; set; }
        }

        private sealed class TestObservable : List<IObserver<object>>, IObservable<object>
        {
            public IDisposable Subscribe(IObserver<object> observer)
            {
                Add(observer);
                return ActionToken.FromDelegate(this, observable => observable.Remove(observer));
            }
        }

        private sealed class Group : ICollectionGroup<int>
        {
            private readonly Func<bool> _cleanup;

            public Group(int value, Func<bool> cleanup)
            {
                Value = value;
                _cleanup = cleanup;
                Items = new List<int>();
            }

            public int Value { get; }

            public IList<int> Items { get; }

            public override string ToString() => Value.ToString();

            public bool TryCleanup() => _cleanup();
        }
    }
}