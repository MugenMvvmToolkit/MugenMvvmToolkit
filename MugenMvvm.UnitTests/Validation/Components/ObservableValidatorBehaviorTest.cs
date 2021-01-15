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

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ObservableValidatorBehaviorTest : UnitTestBase
    {
        [Fact]
        public void ShouldNotifyViewModelOnErrorsChanged()
        {
            string propertyName = "test";
            var invokeCount = 0;
            var messenger = new Messenger();
            var validator = new Validator();
            messenger.AddComponent(new TestMessagePublisherComponent(messenger)
            {
                TryPublish = context =>
                {
                    ++invokeCount;
                    context.Sender.ShouldEqual(validator);
                    context.Message.ShouldEqual(propertyName);
                    return true;
                }
            });
            var vmManager = new ViewModelManager();
            vmManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (_, _, _) => messenger
            });
            var vm = new TestViewModelBase(vmManager);
            vm.Messenger.ShouldEqual(messenger);
            validator.AddComponent(new ObservableValidatorBehavior(vm));

            invokeCount.ShouldEqual(0);
            validator.GetComponents<IValidatorListener>().OnErrorsChanged(validator, vm, propertyName, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldValidateOnPropertyChange()
        {
            string propertyName = "test";
            var invokeCount = 0;
            var vm = new TestViewModelBase();
            var validator = new Validator();
            validator.AddComponent(new TestValidatorComponent
            {
                ValidateAsync = (_, s, _, _) =>
                {
                    ++invokeCount;
                    s.ShouldEqual(propertyName);
                    return Task.CompletedTask;
                }
            });
            var component = new ObservableValidatorBehavior(vm);
            validator.AddComponent(component);

            invokeCount.ShouldEqual(0);
            vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);

            component.Dispose();
            vm.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            invokeCount.ShouldEqual(1);
        }
    }
}