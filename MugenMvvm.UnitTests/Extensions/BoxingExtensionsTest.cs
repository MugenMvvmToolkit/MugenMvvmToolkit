using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Extensions
{
    public class BoxingExtensionsTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void BoxBoolShouldUseCache()
        {
            BoxingExtensions.Box((bool?) null).ShouldBeNull();

            var boxTrue = BoxingExtensions.Box(true);
            boxTrue.ShouldEqual(true);
            ReferenceEquals(BoxingExtensions.Box(true), boxTrue);

            var boxFalse = BoxingExtensions.Box(false);
            boxFalse.ShouldEqual(false);
            ReferenceEquals(BoxingExtensions.Box(false), boxFalse);

            boxTrue = BoxingExtensions.Box((bool?) true);
            boxTrue.ShouldEqual(true);
            ReferenceEquals(BoxingExtensions.Box(true), boxTrue);

            boxFalse = BoxingExtensions.Box((bool?) false);
            boxFalse.ShouldEqual(false);
            ReferenceEquals(BoxingExtensions.Box(false), boxFalse);
        }

        [Fact]
        public void BoxByteShouldUseCache()
        {
            BoxingExtensions.Box((byte?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((byte?) i)!);
            }

            objects.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void BoxSByteShouldUseCache()
        {
            BoxingExtensions.Box((sbyte?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (var i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((sbyte?) i)!);
            }

            objects.Count.ShouldEqual(byte.MaxValue);
        }

        [Fact]
        public void BoxUShortShouldUseCache()
        {
            BoxingExtensions.Box((ushort?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (ushort i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((ushort?) i)!);
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
            BoxingExtensions.Box((short?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (short i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((short?) i)!);
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
            BoxingExtensions.Box((uint?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (uint i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((uint?) i)!);
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
            BoxingExtensions.Box((int?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (var i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((int?) i)!);
            }

            objects.Count.ShouldEqual(BoxingExtensions.CacheSize * 2);

            objects.Clear();
            objects.Add(BoxingExtensions.Box(BoxingExtensions.CacheSize + 1));
            objects.Add(BoxingExtensions.Box(BoxingExtensions.CacheSize + 1));
            objects.Count.ShouldEqual(2);
        }

        [Fact]
        public void BoxULongShouldUseCache()
        {
            BoxingExtensions.Box((ulong?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (ulong i = 0; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((ulong?) i)!);
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
            BoxingExtensions.Box((long?) null).ShouldBeNull();
            var objects = new HashSet<object>(ReferenceEqualityComparer.Instance);
            for (long i = -BoxingExtensions.CacheSize; i < BoxingExtensions.CacheSize; i++)
            {
                var item = BoxingExtensions.Box(i);
                item.ShouldEqual(i);

                objects.Add(item);
                objects.Add(BoxingExtensions.Box(i));
                objects.Add(BoxingExtensions.Box((long?) i)!);
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