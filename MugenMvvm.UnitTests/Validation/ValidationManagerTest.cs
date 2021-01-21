using System;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidationManagerTest : ComponentOwnerTestBase<ValidationManager>
    {
        [Fact]
        public void GetAggregatorValidatorShouldThrowNoComponents()
        {
            var provider = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => provider.GetValidator(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetValidatorShouldBeHandledByComponents(int componentCount)
        {
            var provider = GetComponentOwner();
            var validator = new Validator();
            ItemOrIReadOnlyList<object> requests = this;
            var count = 0;
            var listenerCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestValidatorProviderComponent(provider)
                {
                    TryGetValidator = (o, meta) =>
                    {
                        ++count;
                        o.ShouldEqual(requests);
                        meta.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return validator;
                        return null;
                    },
                    Priority = -i
                };
                provider.AddComponent(component);
                provider.AddComponent(new TestValidationManagerListener(provider)
                {
                    OnValidatorCreated = (v, o, meta) =>
                    {
                        ++listenerCount;
                        v.ShouldEqual(validator);
                        o.ShouldEqual(requests);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            provider.GetValidator(requests, DefaultMetadata).ShouldEqual(validator);
            componentCount.ShouldEqual(count);
            listenerCount.ShouldEqual(count);
        }

        protected override ValidationManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new(collectionProvider);
    }
}