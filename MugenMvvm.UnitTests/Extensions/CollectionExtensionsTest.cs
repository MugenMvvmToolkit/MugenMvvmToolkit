using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Collections;
using MugenMvvm.UnitTests.Models.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Extensions
{
    [Collection(SharedContext)]
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

            _collection.ConfigureDecorators<TestObservable>().AutoRefreshOnObservable(observable => observable, this);
            for (var i = 0; i < count; i++)
            {
                var testObservable = (TestObservable) _collection[i];
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
                var changedModel = (TestObservable) oldItems[i];
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

            _collection.ConfigureDecorators<TestNotifyPropertyChangedModel>().AutoRefreshOnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property), this);
            for (var i = 0; i < count; i++)
            {
                ((TestNotifyPropertyChangedModel) _collection[i]).OnPropertyChanged(nameof(TestNotifyPropertyChangedModel.Property));
                ((TestNotifyPropertyChangedModel) _collection[i]).OnPropertyChanged("Test");
            }

            changedEvents.Count.ShouldEqual(count);
            changedEvents.ShouldEqualUnordered(_collection);

            changedEvents.Clear();
            var oldItems = _collection.ToArray();
            _collection.Clear();

            for (var i = 0; i < count; i++)
            {
                var changedModel = (TestNotifyPropertyChangedModel) oldItems[i];
                changedModel.HasSubscribers.ShouldBeFalse();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BindToSourceShouldCreateReadOnlyObservableCollection(bool disposeSource)
        {
            var readOnlyObservableCollection = (ReadOnlyObservableCollection<object>) _collection.BindToSource(disposeSource);
            readOnlyObservableCollection.Dispose();
            _collection.IsDisposed.ShouldEqual(disposeSource);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BindShouldCreateReadOnlyObservableCollection(bool disposeSource)
        {
            var readOnlyObservableCollection = (DecoratedReadOnlyObservableCollection<object>) _collection.Bind(disposeSource);
            readOnlyObservableCollection.Dispose();
            _collection.IsDisposed.ShouldEqual(disposeSource);
        }

        [Theory]
        [InlineData(10, true)]
        [InlineData(10, false)]
        [InlineData(2, true)]
        public void ConfigureDecoratorsShouldUseLatestPriorityAllowNull(int step, bool allowNull)
        {
            _collection.ConfigureDecorators(allowNull, 10).Priority.ShouldEqual(10);
            _collection.ConfigureDecorators(allowNull).AllowNull.ShouldEqual(allowNull);
            _collection.ConfigureDecorators(step: step).Step.ShouldEqual(step);

            _collection.ConfigureDecorators().Priority.ShouldEqual(0);

            _collection.AddComponent(new TestCollectionDecorator {Priority = 0});
            _collection.ConfigureDecorators(step: step).Priority.ShouldEqual(-step);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void CountShouldBeValid(bool withCondition, bool immutable)
        {
            const int count = 1000;
            var countValue = 0;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -10000;
            _collection.ConfigureDecorators<IntValue>().Count(i => countValue = i, condition, immutable);
            if (!withCondition)
                condition = _ => true;
            Action assert = () => countValue.ShouldEqual(_collection.OfType<IntValue>().Count(condition!));

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue {Value = next});

                assert();
            }

            for (int i = 0; i < count / 2; i++)
            {
                _collection[i] = new IntValue {Value = _random.Next(int.MinValue, int.MaxValue)};
                assert();
            }

            if (!immutable)
            {
                foreach (var value in _collection.OfType<IntValue>())
                {
                    value.Value = _random.Next(int.MinValue, int.MaxValue);
                    _collection.RaiseItemChanged(value);
                    assert();
                }
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue {Value = _random.Next()}));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue) _collection[i];
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
        public void GroupByShouldBeValid1(bool flatten)
        {
            var group1Cleanup = 0;
            var group1 = new Group(0, () =>
            {
                ++group1Cleanup;
                return true;
            }, ComponentCollectionManager);
            var group2Cleanup = 0;
            var group2 = new Group(1, () =>
            {
                ++group2Cleanup;
                return false;
            }, ComponentCollectionManager);
            _collection.ConfigureDecorators<int>()
                       .GroupBy(i => Optional.Get(i % 2), i => i == 0 ? group1 : group2, SortingComparerBuilder.Get<Group>().Descending(group => group.Value).Build(), null,
                           flatten);

            for (var i = 0; i < 10; i++)
                _collection.Add(i);

            group1.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 == 0));
            group2.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 != 0));

            _collection.DecoratedItems().ShouldEqual(flatten ? group2.Items.Concat(group1.Items).OfType<object>() : new[] {group2, group1});

            _collection.Clear();
            group1Cleanup.ShouldEqual(1);
            group1.Items.ShouldNotBeEmpty();

            group2Cleanup.ShouldEqual(1);
            group2.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GroupByShouldBeValid2(bool flatten)
        {
            var group1Cleanup = 0;
            var group1 = new Group(0, () =>
            {
                ++group1Cleanup;
                return true;
            }, ComponentCollectionManager);
            var group2Cleanup = 0;
            var group2 = new Group(1, () =>
            {
                ++group2Cleanup;
                return false;
            }, ComponentCollectionManager);
            _collection.ConfigureDecorators<int>()
                       .GroupBy(i => Optional.Get(i % 2), i => i == 0 ? group1 : group2, builder => builder.Descending(g => g.Value), null, flatten);

            for (var i = 0; i < 10; i++)
                _collection.Add(i);

            group1.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 == 0));
            group2.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 != 0));

            _collection.DecoratedItems().ShouldEqual(flatten ? group2.Items.Concat(group1.Items).OfType<object>() : new[] {group2, group1});

            _collection.Clear();
            group1Cleanup.ShouldEqual(1);
            group1.Items.ShouldNotBeEmpty();

            group2Cleanup.ShouldEqual(1);
            group2.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GroupByShouldBeValid3(bool flatten)
        {
            var group1Cleanup = 0;
            var group1 = new Group(0, () =>
            {
                ++group1Cleanup;
                return true;
            }, ComponentCollectionManager);
            var group2Cleanup = 0;
            var group2 = new Group(1, () =>
            {
                ++group2Cleanup;
                return false;
            }, ComponentCollectionManager);
            _collection.ConfigureDecorators<int>()
                       .GroupBy(i => Optional.Get(i % 2), i => i == 0 ? group1 : group2, group => group.Value, flatten: flatten);

            for (var i = 0; i < 10; i++)
                _collection.Add(i);

            group1.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 == 0));
            group2.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 != 0));

            _collection.DecoratedItems().ShouldEqual(flatten ? group1.Items.Concat(group2.Items).OfType<object>() : new[] {group1, group2});

            _collection.Clear();
            group1Cleanup.ShouldEqual(1);
            group1.Items.ShouldNotBeEmpty();

            group2Cleanup.ShouldEqual(1);
            group2.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GroupByShouldBeValid4(bool flatten)
        {
            var group1Cleanup = 0;
            var group1 = new Group(0, () =>
            {
                ++group1Cleanup;
                return true;
            }, ComponentCollectionManager);
            var group2Cleanup = 0;
            var group2 = new Group(1, () =>
            {
                ++group2Cleanup;
                return false;
            }, ComponentCollectionManager);
            _collection.ConfigureDecorators<int>()
                       .GroupBy(i => Optional.Get(i % 2), i => i == 0 ? group1 : group2, group => group.Value, builder => builder.Descending(group => group), null, flatten);

            for (var i = 0; i < 10; i++)
                _collection.Add(i);

            group1.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 == 0));
            group2.Items.ShouldEqualUnordered(_collection.OfType<int>().Where(i => i % 2 != 0));

            _collection.DecoratedItems().ShouldEqual(flatten ? group2.Items.Concat(group1.Items).OfType<object>() : new[] {group2, group1});

            _collection.Clear();
            group1Cleanup.ShouldEqual(1);
            group1.Items.ShouldNotBeEmpty();

            group2Cleanup.ShouldEqual(1);
            group2.Items.ShouldBeEmpty();
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void MaxShouldBeValid(bool withCondition, bool immutable)
        {
            const int count = 1000;
            IntValue? maxValueItem = null;
            var maxValue = int.MinValue;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value < 10000;
            _collection.ConfigureDecorators<IntValue>().Max((i, v) =>
            {
                maxValueItem = i;
                maxValue = v;
            }, i => i.Value, maxValue, condition, null, immutable);
            if (!withCondition)
                condition = _ => true;
            Action assert = () =>
            {
                var ints = _collection.OfType<IntValue>().Where(condition!);
                if (!ints.Any())
                {
                    maxValueItem.ShouldBeNull();
                    maxValue.ShouldEqual(int.MinValue);
                    return;
                }

                ints.Max(value => value.Value).ShouldEqual(maxValue);
                ints.First(value => value.Value == maxValue).ShouldEqual(maxValueItem);
            };

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue {Value = next});

                assert();
            }

            for (int i = 0; i < count / 2; i++)
            {
                _collection[i] = new IntValue {Value = _random.Next(int.MinValue, int.MaxValue)};
                assert();
            }

            if (!immutable)
            {
                foreach (var value in _collection.OfType<IntValue>())
                {
                    value.Value = _random.Next(int.MinValue, int.MaxValue);
                    _collection.RaiseItemChanged(value);
                    assert();
                }
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue {Value = _random.Next()}));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue) _collection[i];
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
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void MinShouldBeValid(bool withCondition, bool immutable)
        {
            const int count = 1000;
            var minValue = int.MaxValue;
            IntValue? minValueItem = null;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -10000;
            _collection.ConfigureDecorators<IntValue>().Min((i, v) =>
            {
                minValueItem = i;
                minValue = v;
            }, i => i.Value, minValue, condition, null, immutable);
            if (!withCondition)
                condition = _ => true;
            Action assert = () =>
            {
                var ints = _collection.OfType<IntValue>().Where(condition!);
                if (!ints.Any())
                {
                    minValueItem.ShouldBeNull();
                    minValue.ShouldEqual(int.MaxValue);
                    return;
                }

                ints.Min(value => value.Value).ShouldEqual(minValue);
                ints.First(value => value.Value == minValue).ShouldEqual(minValueItem);
            };

            for (var i = 1; i < count; i++)
            {
                var next = _random.Next(int.MinValue, int.MaxValue);
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new IntValue {Value = next});

                assert();
            }

            for (int i = 0; i < count / 2; i++)
            {
                _collection[i] = new IntValue {Value = _random.Next(int.MinValue, int.MaxValue)};
                assert();
            }

            if (!immutable)
            {
                foreach (var value in _collection.OfType<IntValue>())
                {
                    value.Value = _random.Next(int.MinValue, int.MaxValue);
                    _collection.RaiseItemChanged(value);
                    assert();
                }
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue {Value = _random.Next()}));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue) _collection[i];
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
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void SumShouldBeValid1(bool withCondition, bool immutable)
        {
            const int count = 1000;
            var sum = 0;
            Func<IntValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -1000;
            _collection.ConfigureDecorators<IntValue>().Sum(i => sum = i, arg => arg.Value, condition, immutable);
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
                    _collection.Add(new IntValue {Value = next});

                assert();
            }

            for (int i = 0; i < count / 2; i++)
            {
                _collection[i] = new IntValue {Value = Next()};
                assert();
            }

            if (!immutable)
            {
                foreach (var value in _collection.OfType<IntValue>())
                {
                    value.Value = Next();
                    _collection.RaiseItemChanged(value);
                    assert();
                }
            }

            var values = new List<IntValue>(Enumerable.Range(0, count / 2).Select(_ => new IntValue {Value = Next()}));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (IntValue) _collection[i];
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
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void SumShouldBeValid2(bool withCondition, bool immutable)
        {
            const int count = 1000;
            decimal sum = 0;
            Func<DecimalValue, bool>? condition = null;
            if (withCondition)
                condition = value => value.Value > -1000;
            _collection.ConfigureDecorators<DecimalValue>().Sum(i => sum = i, arg => arg.Value, condition, immutable);
            if (!withCondition)
                condition = _ => true;

            decimal Next()
            {
                return (decimal) (_random.NextDouble() * _random.Next(-10000, 10000));
            }

            Action assert = () => sum.ShouldEqual(_collection.OfType<DecimalValue>().Where(condition!).Sum(value => value.Value));
            for (var i = 1; i < count; i++)
            {
                var next = Next();
                if (i % 5 == 0)
                    _collection.RemoveAt(0);
                else
                    _collection.Add(new DecimalValue {Value = next});

                assert();
            }

            for (int i = 0; i < count / 2; i++)
            {
                _collection[i] = new DecimalValue {Value = Next()};
                assert();
            }

            if (!immutable)
            {
                foreach (var value in _collection.OfType<DecimalValue>())
                {
                    value.Value = Next();
                    _collection.RaiseItemChanged(value);
                    assert();
                }
            }

            var values = new List<DecimalValue>(Enumerable.Range(0, count / 2).Select(_ => new DecimalValue {Value = Next()}));
            for (var i = 0; i < count / 2; i++)
            {
                var o = (DecimalValue) _collection[i];
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
            ISelectedItemTracker<object> selectedItemTracker = null!;
            _collection.ConfigureDecorators().TrackSelectedItem<object, object?>(out selectedItemTracker, this, (o, s, _) =>
            {
                s.ShouldEqual(this);
                o.ShouldEqual(selectedItemTracker.SelectedItem);
                selectedItem = o;
            }, (ints, i) =>
            {
                if (!ints.Any())
                    return int.MaxValue;
                i.ShouldEqual(selectedItemTracker.SelectedItem);
                return ints.Max(j => (int) j);
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

        [Theory]
        [InlineData(true, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void AllShouldBeValid(bool hasCondition, bool immutable)
        {
            Func<IntValue, bool> predicate = v => !hasCondition || v.Value < 1000;
            Func<IntValue, bool> selector = v => v.Value > 0;
            var value = false;

            void Assert()
            {
                _collection.OfType<IntValue>().Where(predicate).All(selector).ShouldEqual(value);
            }

            _collection.ConfigureDecorators<IntValue>().All(b => value = b, selector, hasCondition ? predicate : null, immutable);

            _collection.Add(new IntValue {Value = -1});
            Assert();

            _collection.Add(new IntValue {Value = 1});
            Assert();

            _collection.RemoveAt(0);
            Assert();

            _collection.Clear();
            Assert();

            _collection.Add(new IntValue {Value = int.MaxValue});
            Assert();

            _collection.Add(new IntValue {Value = 1});
            Assert();

            if (!immutable)
            {
                ((IntValue) _collection[0]).Value = -1;
                ((IntValue) _collection[1]).Value = -1;
                _collection.RaiseItemChanged(_collection[0]);
                _collection.RaiseItemChanged(_collection[1]);
                Assert();

                ((IntValue) _collection[1]).Value = 1;
                _collection.RaiseItemChanged(_collection[1]);
                Assert();

                ((IntValue) _collection[0]).Value = 1;
                _collection.RaiseItemChanged(_collection[0]);
                Assert();
            }

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AnyShouldBeValid(bool hasSelector)
        {
            Func<IntValue, bool> selector = v => !hasSelector || v.Value > 0;
            var value = false;

            void Assert()
            {
                _collection.OfType<IntValue>().Any(selector).ShouldEqual(value);
            }

            _collection.ConfigureDecorators<IntValue>().Any(b => value = b, hasSelector ? selector : null);

            _collection.Add(new IntValue {Value = -1});
            Assert();

            _collection.Add(new IntValue {Value = 1});
            Assert();

            _collection.RemoveAt(_collection.Count - 1);
            Assert();

            _collection.Clear();
            Assert();

            _collection.Add(new IntValue {Value = int.MaxValue});
            Assert();

            _collection.Add(new IntValue {Value = 1});
            Assert();

            ((IntValue) _collection[0]).Value = -1;
            ((IntValue) _collection[1]).Value = -1;
            _collection.RaiseItemChanged(_collection[0]);
            _collection.RaiseItemChanged(_collection[1]);
            Assert();

            ((IntValue) _collection[1]).Value = 1;
            _collection.RaiseItemChanged(_collection[1]);
            Assert();

            ((IntValue) _collection[0]).Value = 1;
            _collection.RaiseItemChanged(_collection[0]);
            Assert();

            _collection.Clear();
            Assert();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FirstOrDefaultShouldBeValid(bool hasCondition)
        {
            int value = 0;
            bool hasValue = false;
            Func<int, bool>? predicate = hasCondition ? i => i % 2 == 0 : null;
            _collection.ConfigureDecorators<int>().FirstOrDefault(v =>
            {
                value = v.GetValueOrDefault();
                hasValue = v.HasValue;
            }, predicate, out var token);

            void Assert()
            {
                var ints = _collection.DecoratedItems().OfType<int>().Where(predicate ?? (_ => true));
                ints.FirstOrDefault().ShouldEqual(value);
                hasValue.ShouldEqual(ints.Any());
            }

            for (int i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
                _collection.Insert(0, i.ToString());
                Assert();
                _collection.Insert(0, i);
                Assert();
            }

            _collection.Clear();
            Assert();

            token.Dispose();
            _collection.Add(1);
            value.ShouldEqual(0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LastOrDefaultShouldBeValid(bool hasCondition)
        {
            int value = 0;
            bool hasValue = false;
            Func<int, bool>? predicate = hasCondition ? i => i % 2 == 0 : null;
            _collection.ConfigureDecorators<int>().LastOrDefault(v =>
            {
                value = v.GetValueOrDefault();
                hasValue = v.HasValue;
            }, predicate, out var token);

            void Assert()
            {
                var ints = _collection.DecoratedItems().OfType<int>().Where(predicate ?? (_ => true));
                ints.LastOrDefault().ShouldEqual(value);
                hasValue.ShouldEqual(ints.Any());
            }

            for (int i = 0; i < 100; i++)
            {
                _collection.Add(i);
                Assert();
                _collection.Add(i.ToString());
                Assert();
                _collection.Add(i);
                Assert();
            }

            _collection.Clear();
            Assert();

            token.Dispose();
            _collection.Add(1);
            value.ShouldEqual(0);
        }

        [Fact]
        public void SealedShouldThrowOnDecoratorChanged1()
        {
            _collection.ConfigureDecorators().Sealed(out var token);
            ShouldThrow<InvalidOperationException>(() => _collection.AddComponent(new TestCollectionDecorator()));
            _collection.AddComponent(new ListenerDecorator());
            token.Dispose();
            _collection.AddComponent(new TestCollectionDecorator());
        }

        [Fact]
        public void SealedShouldThrowOnDecoratorChanged2()
        {
            var listener = new ListenerDecorator();
            var decorator = new TestCollectionDecorator();
            _collection.AddComponent(listener);
            _collection.AddComponent(decorator);
            _collection.ConfigureDecorators().Sealed(out var token);
            ShouldThrow<InvalidOperationException>(() => _collection.RemoveComponent(decorator));
            _collection.Remove(listener);
            token.Dispose();
            _collection.RemoveComponent(decorator);
        }

        [Fact]
        public void SealedShouldThrowOnDecoratorChanged3()
        {
            var listener = new ListenerDecorator();
            var decorator = new TestCollectionDecorator();
            _collection.AddComponent(listener);
            _collection.AddComponent(decorator);
            _collection.ConfigureDecorators().Sealed(out _);
            _collection.Dispose();
        }

        [Fact]
        public void SynchronizeLockerShouldSynchronizeCollectionLockers()
        {
            IReadOnlyObservableCollection target = new SynchronizedObservableCollection<object>(ComponentCollectionManager);
            IReadOnlyObservableCollection source = _collection;
            target.Locker.ShouldNotEqual(source.Locker);
            _collection.ConfigureDecorators().SynchronizeLocker(target, out var token);
            target.Locker.ShouldEqual(source.Locker);

            var locker1 = new TestLocker {Priority = 1};
            var locker2 = new TestLocker {Priority = 2};
            var locker3 = new TestLocker {Priority = 3};
            var locker4 = new TestLocker {Priority = 3};
            target.UpdateLocker(locker1);
            target.Locker.ShouldEqual(locker1);
            source.Locker.ShouldEqual(locker1);

            source.UpdateLocker(locker2);
            target.Locker.ShouldEqual(locker2);
            source.Locker.ShouldEqual(locker2);

            token.Dispose();
            target.UpdateLocker(locker3);
            target.Locker.ShouldEqual(locker3);
            source.Locker.ShouldEqual(locker2);

            source.UpdateLocker(locker4);
            target.Locker.ShouldEqual(locker3);
            source.Locker.ShouldEqual(locker4);
        }

        private sealed class TestLocker : ILocker
        {
            public int Priority { get; set; }

            public void Enter(ref bool lockTaken)
            {
                lockTaken = true;
            }

            public void TryEnter(int timeout, ref bool lockTaken)
            {
                lockTaken = true;
            }

            public void Exit()
            {
            }
        }

        private sealed class ListenerDecorator : TestCollectionDecorator, IListenerCollectionDecorator
        {
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

            public Group(int value, Func<bool> cleanup, IComponentCollectionManager collectionManager)
            {
                Value = value;
                _cleanup = cleanup;
                Items = new SynchronizedObservableCollection<int>(collectionManager);
            }

            public int Value { get; }

            public IList<int> Items { get; }

            public override string ToString() => Value.ToString();

            public bool TryCleanup() => _cleanup();
        }
    }
}