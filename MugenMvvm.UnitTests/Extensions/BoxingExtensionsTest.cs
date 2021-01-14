using System.Collections.Generic;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Extensions
{
    public class BoxingExtensionsTest : UnitTestBase
    {
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
    }
}