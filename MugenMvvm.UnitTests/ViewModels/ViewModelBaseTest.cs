using System;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels
{
    public class ViewModelBaseTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void RegisterDisposeTokenShouldInvokeTokenOnDispose(int count)
        {
            var invokedCount = 0;
            var vm = new TestViewModelBase();
            for (var i = 0; i < count; i++)
                vm.RegisterDisposeToken(new ActionToken((s1, s2) => ++invokedCount));

            invokedCount.ShouldEqual(0);
            vm.Dispose();
            invokedCount.ShouldEqual(count);

            vm.Dispose();
            invokedCount.ShouldEqual(count);

            invokedCount = 0;
            for (var i = 0; i < count; i++)
                vm.RegisterDisposeToken(new ActionToken((s1, s2) => ++invokedCount));
            invokedCount.ShouldEqual(count);
        }

        [Fact]
        public void ShouldNotifyLifecycle()
        {
            var createdState = 0;
            var disposingState = 0;
            var disposedState = 0;
            IViewModelBase? viewModel = null;
            using var t = MugenService.AddComponent(new TestViewModelLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (vm, state, arg3, arg4) =>
                {
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

            var vm = new TestViewModelBase();
            vm.ShouldEqual(viewModel);

            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(0);
            disposedState.ShouldEqual(0);

            vm.Dispose();
            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(1);
            disposedState.ShouldEqual(1);

            vm.Dispose();
            createdState.ShouldEqual(1);
            disposingState.ShouldEqual(1);
            disposedState.ShouldEqual(1);
        }

        [Fact]
        public void BusyManagerShouldBeOptional()
        {
            var viewModel = new TestViewModelBase();
            var hasService = (IHasService<IBusyManager>) viewModel;
            hasService.ServiceOptional.ShouldBeNull();
            viewModel.TryGetService<IBusyManager>(true).ShouldBeNull();
        }

        [Fact]
        public void ShouldUseBusyManager()
        {
            var beginBusyCount = 0;
            var stateChangedCount = 0;
            var isBusyCount = 0;
            var busyTokenCount = 0;
            var busyManager = new BusyManager();
            busyManager.AddComponent(new BusyManagerComponent());
            var viewModel = new TestViewModelBase
            {
                OnBeginBusyHandler = (manager, token, arg3) =>
                {
                    manager.ShouldEqual(busyManager);
                    ++beginBusyCount;
                },
                OnBusyStateChangedHandler = (manager, context) =>
                {
                    manager.ShouldEqual(busyManager);
                    ++stateChangedCount;
                }
            };
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(viewModel.IsBusy))
                    ++isBusyCount;
                else if (args.PropertyName == nameof(viewModel.BusyToken))
                    ++busyTokenCount;
            };
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyToken.ShouldBeNull();
            using var t = MugenService.AddComponent(new TestViewModelServiceResolverComponent
            {
                TryGetService = (vm, o, arg3) =>
                {
                    viewModel.ShouldEqual(vm);
                    o.ShouldEqual(typeof(IBusyManager));
                    return busyManager;
                }
            });

            viewModel.BusyManager.ShouldEqual(busyManager);
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyToken.ShouldBeNull();

            var beginBusy = viewModel.BusyManager.BeginBusy();
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

        #endregion
    }
}