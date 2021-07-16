using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Validation;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelServiceProviderTest : UnitTestBase
    {
        public ViewModelServiceProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ViewModelManager.AddComponent(new ViewModelServiceProvider(ReflectionManager, ValidationManager, ThreadDispatcher, ComponentCollectionManager));
        }

        [Fact]
        public void TryGetServiceShouldReturnBusyManager()
        {
            var service = (IBusyManager)ViewModelManager.TryGetService(new TestViewModel(), typeof(IBusyManager), Metadata)!;
            service.GetComponent<IBusyManagerComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMessenger()
        {
            var service = (IMessenger)ViewModelManager.TryGetService(new TestViewModel(), typeof(IMessenger), Metadata)!;
            service.GetComponent<IMessagePublisherComponent>().ShouldNotBeNull();
            service.GetComponent<IMessengerSubscriberComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMetadataContext() =>
            ViewModelManager.TryGetService(new TestViewModel(), typeof(IMetadataContext), Metadata).ShouldBeType<MetadataContext>();

        [Fact]
        public void TryGetServiceShouldReturnNullUnknownComponent() => ViewModelManager.TryGetService(new TestViewModel(), typeof(object), Metadata).ShouldBeNull();

        [Fact]
        public void TryGetServiceShouldReturnValidator()
        {
            var vm = new TestViewModel();
            var validator = new Validator(null, ComponentCollectionManager);

            ValidationManager.AddComponent(new TestValidatorProviderComponent
            {
                TryGetValidator = (_, o, context) =>
                {
                    o.ShouldEqual(vm);
                    context.ShouldEqual(Metadata);
                    return validator;
                }
            });
            ViewModelManager.TryGetService(vm, typeof(IValidator), Metadata).ShouldEqual(validator);
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        protected override IValidationManager GetValidationManager() => new ValidationManager(ComponentCollectionManager);
    }
}