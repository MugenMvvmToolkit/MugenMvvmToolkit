using System;
using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorProviderTest : ComponentOwnerTestBase<ValidatorProvider>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetValidatorsShouldBeHandledByComponents(int componentCount)
        {
            var provider = GetComponentOwner();
            var validators = new List<IValidator>();
            var count = 0;
            var listenerCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var validator = new InlineValidator<object>();
                validators.Add(validator);
                var component = new TestValidatorProviderComponent
                {
                    TryGetValidators = (o, type, meta) =>
                    {
                        ++count;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidatorProviderTest));
                        meta.ShouldEqual(DefaultMetadata);
                        return new[] {validator};
                    },
                    Priority = -i
                };
                provider.AddComponent(component);
                provider.AddComponent(new TestValidatorProviderListener
                {
                    OnValidatorCreated = (validatorProvider, v, o, type, meta) =>
                    {
                        ++listenerCount;
                        validators.Contains(v).ShouldBeTrue();
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidatorProviderTest));
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            provider.GetValidators(this, DefaultMetadata).ShouldEqual(validators);
            componentCount.ShouldEqual(count);
            listenerCount.ShouldEqual(count * count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetAggregatorValidatorShouldBeHandledByComponents(int componentCount)
        {
            var provider = GetComponentOwner();
            var validator = new AggregatorValidator();
            var count = 0;
            var listenerCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestAggregatorValidatorProviderComponent
                {
                    TryGetAggregatorValidator = (o, type, meta) =>
                    {
                        ++count;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidatorProviderTest));
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
                    OnAggregatorValidatorCreated = (validatorProvider, v, o, type, meta) =>
                    {
                        ++listenerCount;
                        v.ShouldEqual(validator);
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(ValidatorProviderTest));
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            provider.GetAggregatorValidator(this, DefaultMetadata).ShouldEqual(validator);
            componentCount.ShouldEqual(count);
            listenerCount.ShouldEqual(count);
        }

        [Fact]
        public void GetAggregatorValidatorShouldThrowNoComponents()
        {
            var provider = GetComponentOwner();
            ShouldThrow<InvalidOperationException>(() => provider.GetAggregatorValidator(this, DefaultMetadata));
        }

        protected override ValidatorProvider GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new ValidatorProvider(collectionProvider);
        }

        #endregion
    }
}