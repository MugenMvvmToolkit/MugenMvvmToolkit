using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Messaging;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using MugenMvvm.ViewModels;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ObservableValidatorBehaviorTest : UnitTestBase
    {
        private readonly Messenger _messenger;
        private readonly Validator _validator;
        private readonly ViewModelManager _viewModelManager;
        private readonly TestViewModelBase _vm;

        public ObservableValidatorBehaviorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _messenger = new Messenger(ComponentCollectionManager);
            _validator = new Validator(null, ComponentCollectionManager);
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _vm = new TestViewModelBase(_viewModelManager) {ThreadDispatcher = ThreadDispatcher};
            _validator.AddComponent(new ObservableValidatorBehavior(_vm));
        }

        [Fact]
        public void ShouldNotifyViewModelOnErrorsChanged()
        {
            string propertyName = "test";
            var invokeCount = 0;

            _messenger.AddComponent(new TestMessagePublisherComponent(_messenger)
            {
                TryPublish = context =>
                {
                    ++invokeCount;
                    context.Sender.ShouldEqual(_validator);
                    context.Message.ShouldEqual(propertyName);
                    return true;
                }
            });

            _viewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (_, _, _) => _messenger
            });

            _vm.Messenger.ShouldEqual(_messenger);
            invokeCount.ShouldEqual(0);
            _validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(_validator, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldValidateOnPropertyChange()
        {
            string propertyName = "test";
            var invokeCount = 0;

            _validator.AddComponent(new TestValidationHandlerComponent(_validator)
            {
                TryValidateAsync = (s, _, _) =>
                {
                    ++invokeCount;
                    s.ShouldEqual(propertyName);
                    return Task.CompletedTask;
                }
            });

            invokeCount.ShouldEqual(0);
            _vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);

            _validator.Dispose();
            _vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);
        }
    }
}