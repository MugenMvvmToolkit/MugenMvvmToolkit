using System;
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
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetValidatorShouldBeHandledByComponents(int componentCount)
        {
            var provider = GetComponentOwner();
            var validator = new Validator();
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
                        o.ShouldEqual(this);
                        meta.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return validator;
                        return null;
                    },
                    Priority = -i
                };
                provider.AddComponent(component);
                provider.AddComponent(new TestValidatorProviderListener(provider)
                {
                    OnValidatorCreated = (v, o, meta) =>
                    {
                        ++listenerCount;
                        v.ShouldEqual(validator);
                        o.ShouldEqual(this);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            provider.GetValidator(this, DefaultMetadata).ShouldEqual(validator);
            componentCount.ShouldEqual(count);
            listenerCount.ShouldEqual(count);
        }

        [Fact]
        public void GetAggregatorValidatorShouldThrowNoComponents()
        {
            var provider = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => provider.GetValidator(this, DefaultMetadata));
        }

        protected override ValidationManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new ValidationManager(collectionProvider);

        #endregion
    }
}