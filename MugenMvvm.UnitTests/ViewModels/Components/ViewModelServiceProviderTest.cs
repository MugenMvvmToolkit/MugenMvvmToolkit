using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
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
        private readonly ViewModelManager _viewModelManager;
        private readonly ValidationManager _validationManager;

        public ViewModelServiceProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _validationManager = new ValidationManager(ComponentCollectionManager);
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewModelManager.AddComponent(new ViewModelServiceProvider(ReflectionManager, _validationManager, ThreadDispatcher, ComponentCollectionManager));
        }

        [Fact]
        public void TryGetServiceShouldReturnBusyManager()
        {
            var service = (IBusyManager) _viewModelManager.TryGetService(new TestViewModel(), typeof(IBusyManager), DefaultMetadata)!;
            service.GetComponent<IBusyManagerComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMessenger()
        {
            var service = (IMessenger) _viewModelManager.TryGetService(new TestViewModel(), typeof(IMessenger), DefaultMetadata)!;
            service.GetComponent<IMessagePublisherComponent>().ShouldNotBeNull();
            service.GetComponent<IMessengerSubscriberComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnMetadataContext() =>
            _viewModelManager.TryGetService(new TestViewModel(), typeof(IMetadataContext), DefaultMetadata).ShouldBeType<MetadataContext>();

        [Fact]
        public void TryGetServiceShouldReturnNullUnknownComponent() => _viewModelManager.TryGetService(new TestViewModel(), typeof(object), DefaultMetadata).ShouldBeNull();

        [Fact]
        public void TryGetServiceShouldReturnValidator()
        {
            var vm = new TestViewModel();
            var validator = new Validator(null, ComponentCollectionManager);

            _validationManager.AddComponent(new TestValidatorProviderComponent
            {
                TryGetValidator = (o, context) =>
                {
                    o.ShouldEqual(vm);
                    context.ShouldEqual(DefaultMetadata);
                    return validator;
                }
            });
            _viewModelManager.TryGetService(vm, typeof(IValidator), DefaultMetadata).ShouldEqual(validator);
        }
    }
}