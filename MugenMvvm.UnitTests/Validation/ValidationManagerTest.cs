using System;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Tests.Validation;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidationManagerTest : ComponentOwnerTestBase<ValidationManager>
    {
        [Fact]
        public void GetAggregatorValidatorShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => ValidationManager.GetValidator(this, DefaultMetadata));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetValidatorShouldBeHandledByComponents(int componentCount)
        {
            ItemOrIReadOnlyList<object> requests = this;
            var count = 0;
            var listenerCount = 0;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                ValidationManager.AddComponent(new TestValidatorProviderComponent
                {
                    TryGetValidator = (m, o, meta) =>
                    {
                        ++count;
                        m.ShouldEqual(ValidationManager);
                        o.ShouldEqual(requests);
                        meta.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return Validator;
                        return null;
                    },
                    Priority = -i
                });
                ValidationManager.AddComponent(new TestValidationManagerListener
                {
                    OnValidatorCreated = (m, v, o, meta) =>
                    {
                        ++listenerCount;
                        m.ShouldEqual(ValidationManager);
                        v.ShouldEqual(Validator);
                        o.ShouldEqual(requests);
                        meta.ShouldEqual(DefaultMetadata);
                    }
                });
            }

            ValidationManager.GetValidator(requests, DefaultMetadata).ShouldEqual(Validator);
            componentCount.ShouldEqual(count);
            listenerCount.ShouldEqual(count);
        }

        protected override IValidationManager GetValidationManager() => GetComponentOwner(ComponentCollectionManager);

        protected override ValidationManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}