using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Messaging;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using MugenMvvm.ViewModels;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class PropertyChangedValidatorObserverTest : UnitTestBase
    {
        private readonly TestViewModelBase _vm;

        public PropertyChangedValidatorObserverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _vm = new TestViewModelBase(ViewModelManager) { ThreadDispatcher = ThreadDispatcher };
            Validator.AddComponent(new PropertyChangedValidatorObserver(_vm));
        }

        [Fact]
        public void ShouldNotifyViewModelOnErrorsChanged()
        {
            string propertyName = "test";
            var invokeCount = 0;

            Messenger.AddComponent(new TestMessagePublisherComponent
            {
                TryPublish = (_, context) =>
                {
                    ++invokeCount;
                    context.Sender.ShouldEqual(Validator);
                    context.Message.ShouldEqual(propertyName);
                    return true;
                }
            });

            ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (_, _, _, _) => Messenger
            });

            _vm.Messenger.ShouldEqual(Messenger);
            invokeCount.ShouldEqual(0);
            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, propertyName, Metadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldValidateOnPropertyChange()
        {
            string propertyName = "test";
            var invokeCount = 0;

            Validator.AddComponent(new TestValidationHandlerComponent
            {
                TryValidateAsync = (v, s, _, _) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(Validator);
                    s.ShouldEqual(propertyName);
                    return Task.CompletedTask;
                }
            });

            invokeCount.ShouldEqual(0);
            _vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);

            Validator.Dispose();
            _vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);
        }

        protected override IValidationManager GetValidationManager() => new ValidationManager(ComponentCollectionManager);

        protected override IMessenger GetMessenger() => new Messenger(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}