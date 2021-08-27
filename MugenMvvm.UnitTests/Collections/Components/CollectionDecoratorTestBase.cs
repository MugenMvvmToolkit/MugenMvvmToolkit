using System;
using MugenMvvm.Interfaces.Collections;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Collections.Components
{
    public abstract class CollectionDecoratorTestBase : UnitTestBase
    {
        public const int DefaultCount = 100;

        protected CollectionDecoratorTestBase(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public virtual void AddShouldTrackChanges() => AddShouldTrackChangesImpl(GetCollection(), Assert);

        [Fact]
        public virtual void ClearShouldTrackChanges() => ClearShouldTrackChangesImpl(GetCollection(), Assert);

        [Fact]
        public virtual void MoveShouldTrackChanges1() => MoveShouldTrackChanges1Impl(GetCollection(), Assert);

        [Fact]
        public virtual void MoveShouldTrackChanges2() => MoveShouldTrackChanges2Impl(GetCollection(), Assert);

        [Fact]
        public virtual void RemoveShouldTrackChanges() => RemoveShouldTrackChangesImpl(GetCollection(), Assert);

        [Fact]
        public virtual void ReplaceShouldTrackChanges1() => ReplaceShouldTrackChanges1Impl(GetCollection(), Assert);

        [Fact]
        public virtual void ReplaceShouldTrackChanges2() => ReplaceShouldTrackChanges2Impl(GetCollection(), Assert);

        [Fact]
        public virtual void ResetShouldTrackChanges() => ResetShouldTrackChangesImpl(GetCollection(), Assert);

        [Fact]
        public virtual void ShouldTrackChanges() => ShouldTrackChangesImpl(GetCollection(), Assert);

        internal static void AddShouldTrackChangesImpl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
            {
                collection.Add(i);
                assert();
            }

            for (var i = 0; i < 10; i++)
            {
                collection.Insert(i, i);
                assert();
            }
        }

        internal static void ClearShouldTrackChangesImpl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
            assert();

            collection.Clear();
            assert();
        }

        internal static void MoveShouldTrackChanges1Impl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
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

        internal static void MoveShouldTrackChanges2Impl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
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

        internal static void RemoveShouldTrackChangesImpl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
            assert();

            for (var i = 0; i < 20; i++)
            {
                collection.Remove(i);
                assert();
            }

            for (var i = 0; i < 10; i++)
            {
                collection.RemoveAt(i);
                assert();
            }

            var count = collection.Count;
            for (var i = 0; i < count; i++)
            {
                collection.RemoveAt(0);
                assert();
            }
        }

        internal static void ReplaceShouldTrackChanges1Impl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
            assert();

            for (var i = 0; i < collection.Count; i++)
            {
                collection[i] = i + 101;
                assert();
            }
        }

        internal static void ReplaceShouldTrackChanges2Impl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount / 2; i++)
                collection.Add(i);
            assert();

            for (var i = 0; i < collection.Count / 2; i++)
            for (var j = collection.Count / 2; j < collection.Count; j++)
            {
                collection[i] = collection[j];
                assert();
            }
        }

        internal static void ResetShouldTrackChangesImpl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < DefaultCount; i++)
                collection.Add(i);
            assert();

            collection.Reset(new object[] { 1, 2, 3, 4, 5 });
            assert();
        }

        internal static void ShouldTrackChangesImpl(IObservableCollection<object> collection, Action assert)
        {
            for (var i = 0; i < 4; i++)
            {
                collection.Add(1);
                assert();

                collection.Insert(1, 2);
                assert();

                collection.Move(0, 1);
                assert();

                collection.Move(1, 0);
                assert();

                collection.Remove(2);
                assert();

                collection.RemoveAt(0);
                assert();

                collection.Reset(new object[] { 1, 2, 3, 4, 5, i });
                assert();

                collection[0] = 200;
                assert();

                collection[3] = 3;
                assert();

                collection.Move(0, collection.Count - 1);
                assert();

                collection.Move(0, collection.Count - 2);
                assert();

                collection[i] = i;
                assert();
            }

            collection.Clear();
            assert();
        }

        protected abstract IObservableCollection<object> GetCollection();

        protected abstract void Assert();
    }
}