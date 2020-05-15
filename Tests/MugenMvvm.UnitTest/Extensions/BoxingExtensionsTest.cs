using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Extensions
{
    public class BoxingExtensionsTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void BoxBoolShouldUseCache()
        {
            var boxTrue = BoxingExtensions.Box(true);
            boxTrue.ShouldEqual(true);
            ReferenceEquals(BoxingExtensions.Box(true), boxTrue);

            var boxFalse = BoxingExtensions.Box(false);
            boxFalse.ShouldEqual(false);
            ReferenceEquals(BoxingExtensions.Box(false), boxFalse);
        }

        [Fact]
        public void BoxByteShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void BoxSByteShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (var i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void BoxUShortShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (ushort i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((ushort) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((ushort) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxShortShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (short i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((short) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((short) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxUIntShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (uint i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((uint) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((uint) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxIntShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (var i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((int) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((int) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxULongShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (ulong i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((ulong) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((ulong) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxLongShouldUseCache()
        {
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (long i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            objects.Clear();
            objects.Add(BoxingExtensions.Box((long) (BoxingExtensions.CacheSize + 1)));
            objects.Add(BoxingExtensions.Box((long) (BoxingExtensions.CacheSize + 1)));
            objects.Count.ShouldEqual(2);
        }

        #endregion
    }
}