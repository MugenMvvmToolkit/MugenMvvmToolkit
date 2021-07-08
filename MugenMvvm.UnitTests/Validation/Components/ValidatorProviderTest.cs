using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using MugenMvvm.ViewModels;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidatorProviderTest : UnitTestBase
    {
        private readonly ValidatorProvider _provider;

        public ValidatorProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new ValidatorProvider(ComponentCollectionManager);
            ValidationManager.AddComponent(_provider);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetValidatorShouldReturnValidator1(bool isAsync)
        {
            _provider.AsyncValidationEnabled = isAsync;
            _provider.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = ValidationManager.TryGetValidator(this, DefaultMetadata)!;
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(isAsync ? 3 : 2);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<ValidatorErrorManager>().Count.ShouldEqual(1);
            if (isAsync)
                validator.Components.Get<AsyncValidationBehavior>().Count.ShouldEqual(1);
        }

        [Fact]
        public void TryGetValidatorShouldReturnValidator2()
        {
            var target = new TestViewModelBase(ViewModelManager);
            _provider.Priority.ShouldEqual(ValidationComponentPriority.ValidatorProvider);
            var validator = ValidationManager.TryGetValidator(target, DefaultMetadata)!;
            validator.ShouldBeType<Validator>();
            validator.Components.Count.ShouldEqual(3);
            validator.Components.Get<CycleHandlerValidatorBehavior>().Count.ShouldEqual(1);
            validator.Components.Get<PropertyChangedValidatorObserver>().Count.ShouldEqual(1);
            validator.Components.Get<ValidatorErrorManager>().Count.ShouldEqual(1);
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}