using System;
using MugenMvvm.Bindings.Converting;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Bindings.Converting;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Converting
{
    public class GlobalValueConverterTest : ComponentOwnerTestBase<GlobalValueConverter>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConvertShouldBeHandledByComponents(int componentCount)
        {
            var value = new object();
            var result = new object();
            var type = typeof(string);
            var member = new object();
            var invokeCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestGlobalValueConverterComponent
                {
                    TryConvert = (IGlobalValueConverter c, ref object? o, Type type1, object? arg3, IReadOnlyMetadataContext? arg4) =>
                    {
                        ++invokeCount;
                        c.ShouldEqual(GlobalValueConverter);
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
                GlobalValueConverter.AddComponent(component);
            }

            GlobalValueConverter.Convert(value, type, member, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override IGlobalValueConverter GetGlobalValueConverter() => GetComponentOwner(ComponentCollectionManager);

        protected override GlobalValueConverter GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}