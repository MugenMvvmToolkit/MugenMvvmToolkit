using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Validation.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
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
                var component = new TestValidatorProviderComponent
                {
                    TryGetValidator = (o, type, meta) =>
                    {
                        ++count;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidationManagerTest));
                        meta.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return validator;
                        return null;
                    },
                    Priority = -i
                };
                provider.AddComponent(component);
                provider.AddComponent(new TestValidatorProviderListener
                {
                    OnValidatorCreated = (validatorProvider, v, o, type, meta) =>
                    {
                        ++listenerCount;
                        v.ShouldEqual(validator);
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidationManagerTest));
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

        protected override ValidationManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new ValidationManager(collectionProvider);
        }

        #endregion
    }
}