﻿using System;
using MugenMvvm.Bindings.Convert;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Bindings.Convert.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Convert
{
    public class GlobalValueConverterTest : ComponentOwnerTestBase<GlobalValueConverter>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConvertShouldBeHandledByComponents(int componentCount)
        {
            var converter = new GlobalValueConverter();
            var value = new object();
            var result = new object();
            var type = typeof(string);
            var member = new object();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestGlobalValueConverterComponent(converter)
                {
                    TryConvert = (ref object? o, Type type1, object? arg3, IReadOnlyMetadataContext? arg4) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(value);
                        type1.ShouldEqual(type);
                        arg3.ShouldEqual(member);
                        arg4.ShouldEqual(DefaultMetadata);
                        if (isLast)
                        {
                            o = result;
                            return true;
                        }

                        return false;
                    },
                    Priority = -i
                };
                converter.AddComponent(component);
            }

            converter.Convert(value, type, member, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override GlobalValueConverter GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);
    }
}