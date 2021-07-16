using System;
using System.ComponentModel;
using MugenMvvm.Busy.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels
{
    [Collection(SharedContext)]
    public class ViewModelBaseTest : UnitTestBase
    {
        private TestViewModelBase _viewModel;

        public ViewModelBaseTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestViewModelBase(ViewModelManager) { ThreadDispatcher = ThreadDispatcher };
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void BusyManagerShouldBeOptional()
        {
            var hasService = (IHasService<IBusyManager>)_viewModel;
            hasService.GetService(true).ShouldBeNull();
            _viewModel.TryGetService<IBusyManager>(true).ShouldBeNull();
        }

        [Fact]
        public void GetViewModelShouldPassParentViewModelParameter()
        {
            var type = typeof(TestViewModel);
            var invokeCount = 0;
            var result = new TestViewModel();
            ViewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (v, o, context) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(ViewModelManager);
                    o.ShouldEqual(type);
                    context!.Get(ViewModelMetadata.ParentViewModel).ShouldEqual(_viewModel);
                    return result;
                }
            });

            invokeCount.ShouldEqual(0);
            _viewModel.GetViewModel<TestViewModel>(Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _viewModel.GetViewModel(typeof(TestViewModel), Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void MessengerShouldBeOptional()
        {
            var hasService = (IHasService<IMessenger>)_viewModel;
            hasService.GetService(true).ShouldBeNull();
            _viewModel.TryGetService<IMessenger>(true).ShouldBeNull();
        }

        [Fact]
        public void OnPropertyChangedShouldNotifyMessenger()
        {
            var propertyChangedMessage = new PropertyChangedEventArgs("test");
            var messenger = new Messenger(ComponentCollectionManager);
            var invokeCount = 0;
            messenger.AddComponent(new TestMessagePublisherComponent
            {
                TryPublish = (_, ctx) =>
                {
                    ++invokeCount;
                    ctx.Sender.ShouldEqual(_viewModel);
                    ctx.Message.ShouldEqual(propertyChangedMessage);
                    return true;
                }
            });
            messenger.AddComponent(new TestMessengerSubscriberComponent
            {
                HasSubscribers = (_, _) => true
            });
            using var t = ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (v, vm, o, _) =>
                {
                    v.ShouldEqual(ViewModelManager);
                    _viewModel.ShouldEqual(vm);
                    o.ShouldEqual(typeof(IMessenger));
                    return messenger;
                }
            });

            _viewModel.Messenger.ShouldEqual(messenger);
            invokeCount.ShouldEqual(0);
            _viewModel.OnPropertyChanged(propertyChangedMessage);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RegisterDisposeTokenShouldInvokeTokenOnDispose(int count)
        {
            var invokedCount = 0;
            for (var i = 0; i < count; i++)
                _viewModel.RegisterDisposeToken(ActionToken.FromDelegate((s1, s2) => ++invokedCount));

            invokedCount.ShouldEqual(0);
            _viewModel.Dispose();
            invokedCount.ShouldEqual(count);

            _viewModel.Dispose();
            invokedCount.ShouldEqual(count);

            invokedCount = 0;
            for (var i = 0; i < count; i++)
                _viewModel.RegisterDisposeToken(ActionToken.FromDelegate((s1, s2) => ++invokedCount));
            invokedCount.ShouldEqual(count);
        }

        [Fact]
        public void ShouldNotifyLifecycle()
        {
            var createdState = 0;
            var disposingState = 0;
            var disposedState = 0;
            IViewModelBase? viewModel = null;
            using var t = ViewModelManager.AddComponent(new TestViewModelLifecycleListener
            {
                OnLifecycleChanged = (v, vm, state, arg3, arg4) =>
                {
                    v.ShouldEqual(ViewModelManager);
                    if (viewModel == null)
                        viewModel = vm;
                    else
                        viewModel.ShouldEqual(vm);
                    if (state == ViewModelLifecycleState.Created)
                        ++createdState;
                    else if (state == ViewModelLifecycleState.Disposing)
                        ++disposingState;
                    else if (state == ViewModelLifecycleState.Disposed)
                        ++disposedState;
                    else
                        throw new NotSupportedException();
                }
            });


            _viewModel = new TestViewModelBase(ViewModelManager) { ThreadDispatcher = ThreadDispatcher };
            _viewModel.ShouldEqual(viewModel);

            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(0);
            disposedState.ShouldEqual(0);

            _viewModel.Dispose();
            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(1);
            disposedState.ShouldEqual(1);

            _viewModel.Dispose();
            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(1);
            disposedState.ShouldEqual(1);
        }

        [Fact]
        public void ShouldUseBusyManager()
        {
            var beginBusyCount = 0;
            var stateChangedCount = 0;
            var isBusyCount = 0;
            var busyTokenCount = 0;

            BusyManager.ClearComponents();
            BusyManager.AddComponent(new BusyTokenManager());

            _viewModel.OnBusyStateChangedHandler = (manager, context) =>
            {
                manager.ShouldEqual(BusyManager);
                ++stateChangedCount;
            };
            _viewModel.OnBeginBusyHandler = (manager, token, arg3) =>
            {
                manager.ShouldEqual(BusyManager);
                ++beginBusyCount;
            };
            _viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_viewModel.IsBusy))
                    ++isBusyCount;
                else if (args.PropertyName == nameof(_viewModel.BusyToken))
                    ++busyTokenCount;
            };
            _viewModel.IsBusy.ShouldBeFalse();
            _viewModel.BusyToken.ShouldBeNull();
            ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (v, vm, o, arg3) =>
                {
                    v.ShouldEqual(ViewModelManager);
                    _viewModel.ShouldEqual(vm);
                    o.ShouldEqual(typeof(IBusyManager));
                    return BusyManager;
                }
            });

            _viewModel.BusyManager.ShouldEqual(BusyManager);
            _viewModel.IsBusy.ShouldBeFalse();
            _viewModel.BusyToken.ShouldBeNull();

            var beginBusy = _viewModel.BusyManager.BeginBusy();
            busyTokenCount.ShouldEqual(1);
            isBusyCount.ShouldEqual(1);
            stateChangedCount.ShouldEqual(1);
            beginBusyCount.ShouldEqual(1);

            beginBusy.Dispose();
            busyTokenCount.ShouldEqual(2);
            isBusyCount.ShouldEqual(2);
            stateChangedCount.ShouldEqual(2);
            beginBusyCount.ShouldEqual(1);
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);
    }
}