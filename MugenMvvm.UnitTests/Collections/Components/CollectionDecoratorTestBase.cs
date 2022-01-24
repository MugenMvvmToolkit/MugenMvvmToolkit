using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public abstract class CollectionDecoratorTestBase : UnitTestBase
    {
        public const int DefaultCount = 100;
        private static readonly Func<int, object?> GetDataDefault = i => i == 0 ? null : i;
        private static readonly Action<IList<object?>, int, Func<int, object?>> InitializeDefaultDataDelegate = InitializeDefaultDataImpl;

        protected CollectionDecoratorTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public virtual void AddShouldTrackChanges() => AddShouldTrackChangesImpl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void ClearShouldTrackChanges() => ClearShouldTrackChangesImpl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void MoveShouldTrackChanges1() => MoveShouldTrackChanges1Impl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void MoveShouldTrackChanges2() => MoveShouldTrackChanges2Impl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void RemoveShouldTrackChanges() => RemoveShouldTrackChangesImpl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void ReplaceShouldTrackChanges1() => ReplaceShouldTrackChanges1Impl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void ReplaceShouldTrackChanges2() => ReplaceShouldTrackChanges2Impl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void ResetShouldTrackChanges() => ResetShouldTrackChangesImpl(GetCollection(), Assert, InitializeDefaultData, GetData);

        [Fact]
        public virtual void ShouldTrackChanges() => ShouldTrackChangesImpl(GetCollection(), Assert, GetData);

        internal static void AddShouldTrackChangesImpl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            for (var i = 0; i < DefaultCount; i++)
            {
                collection.Add(getData(i));
                assert();
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Insert(i, getData(i));
                assert();
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Add(getData(i));
                assert();
            }

            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            for (var i = 0; i < DefaultCount; i++)
            {
                collection.Insert(0, getData(i));
                assert();
            }
        }

        internal static void ClearShouldTrackChangesImpl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            collection.Clear();
            assert();
        }

        internal static void MoveShouldTrackChanges1Impl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            for (var i = 0; i < collection.Count - 1; i++)
            {
                collection.Move(i, i + 1);
                assert();
            }

            for (var i = 0; i < collection.Count - 1; i++)
            {
                collection.Move(i + 1, i);
                assert();
            }

            collection.Move(0, collection.Count - 1);
            assert();

            collection.Move(collection.Count - 1, 0);
            assert();
        }

        internal static void MoveShouldTrackChanges2Impl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            for (var i = 1; i < collection.Count - 1; i++)
            {
                collection.Move(i, Math.Min(i * 2 + i, collection.Count - 1));
                assert();
            }

            for (var i = 1; i < collection.Count - 1; i++)
            {
                collection.Move(Math.Min(i * 2 + i, collection.Count - 1), i);
                assert();
            }
        }

        internal static void RemoveShouldTrackChangesImpl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            for (var i = 0; i < 20; i++)
            {
                var item = collection[i];
                collection.Remove(item);
                assert();
            }

            for (var i = 0; i < 10; i++)
            {
                collection.RemoveAt(i);
                assert();
            }

            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            var count = collection.Count;
            for (var i = 0; i < count; i++)
            {
                collection.RemoveAt(0);
                assert();
            }

            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            count = collection.Count;
            for (var i = 0; i < count; i++)
            {
                collection.RemoveAt(collection.Count - 1);
                assert();
            }

            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            var random = new Random();
            while (collection.Count != 0)
            {
                collection.RemoveAt(random.Next(0, collection.Count - 1));
            }
        }

        internal static void ReplaceShouldTrackChanges1Impl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            for (var i = 0; i < collection.Count; i++)
            {
                collection[i] = getData(i + 101);
                assert();
            }
        }

        internal static void ReplaceShouldTrackChanges2Impl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount / 2, getData);
            assert();

            for (var i = 0; i < collection.Count / 2; i++)
            for (var j = collection.Count / 2; j < collection.Count; j++)
            {
                collection[i] = collection[j];
                assert();
            }
        }

        internal static void ResetShouldTrackChangesImpl(IObservableList<object?> collection, Action assert,
            Action<IList<object?>, int, Func<int, object?>>? initializeDefault = null, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(collection, DefaultCount, getData);
            assert();

            var list = new List<object?>();
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(list, DefaultCount, getData);
            (initializeDefault ?? InitializeDefaultDataDelegate).Invoke(list, DefaultCount, getData);
            collection.Reset(list);
            assert();

            collection.Reset(new[] {getData(1), getData(2), getData(3), getData(4), getData(5),});
            assert();
        }

        internal static void ShouldTrackChangesImpl(IObservableList<object?> collection, Action assert, Func<int, object?>? getData = null)
        {
            getData ??= GetDataDefault;
            for (var i = 0; i < 4; i++)
            {
                collection.Add(getData(1));
                assert();

                var data2 = getData(2);
                collection.Insert(1, getData(2));
                assert();

                collection.Move(0, 1);
                assert();

                collection.Move(1, 0);
                assert();

                collection.Remove(data2);
                assert();

                collection.RemoveAt(0);
                assert();

                collection.Reset(new[] {getData(1), getData(2), getData(3), getData(4), getData(5), getData(i)});
                assert();

                collection[0] = getData(200);
                assert();

                collection[3] = getData(3);
                assert();

                collection.Move(0, collection.Count - 1);
                assert();

                collection.Move(0, collection.Count - 2);
                assert();

                collection[i] = getData(i);
                assert();
            }

            collection.Clear();
            assert();
        }

        private static void InitializeDefaultDataImpl(IList<object?> collection, int minCount, Func<int, object?> getData)
        {
            for (var i = 0; i < minCount; i++)
            {
                collection.Add(getData(i));
                collection.Add(i.ToString());
            }
        }

        protected virtual object? GetData(int index) => GetDataDefault(index);

        protected virtual void InitializeDefaultData(IList<object?> collection, int minCount, Func<int, object?> getData) =>
            InitializeDefaultDataImpl(collection, minCount, getData);

        protected abstract IObservableList<object?> GetCollection();

        protected abstract void Assert();
    }
}