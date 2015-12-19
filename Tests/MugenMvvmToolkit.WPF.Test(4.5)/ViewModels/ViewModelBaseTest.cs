using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    public partial class ViewModelBaseTest
    {
        #region Nested types

        public class ViewModelBaseWithCommand : ViewModelBase
        {
            public ICommand NullCommand { get; set; }

            public ICommand CommandWithoutGetter { private get; set; }

            public ICommand Command { get; set; }
        }

        public class OrderViewModelBase : ViewModelBase
        {
            #region Fields

            public readonly List<string> MethodCallCollection = new List<string>();

            #endregion

            #region Overrides of ViewModelBase

            internal override void OnInitializedInternal()
            {
                MethodCallCollection.Add(InitViewModelInternalKey);
                base.OnInitializedInternal();
            }

            protected override void OnInitializing(IDataContext context)
            {
                MethodCallCollection.Add(InititalizingViewModelKey);
                base.OnInitializing(context);
            }

            protected override void OnInitialized()
            {
                MethodCallCollection.Add(InitViewModelKey);
                Disposed += OnDisposed;
                base.OnInitialized();
            }

            private void OnDisposed(object sender, EventArgs eventArgs)
            {
                MethodCallCollection.Add(OnDisposedKey);
            }

            internal override void OnDisposeInternal(bool disposing)
            {
                MethodCallCollection.Add(DisposeInternalKey);
                base.OnDisposeInternal(disposing);
            }

            protected override void OnDispose(bool disposing)
            {
                MethodCallCollection.Add(DisposeKey);
                base.OnDispose(disposing);
            }

            #endregion
        }

        #endregion

        #region Fields

        public const string InititalizingViewModelKey = "InititalizingViewModelKey";
        public const string InitViewModelInternalKey = "InitViewModelInternalKey";
        public const string InitViewModelKey = "InitViewModelKey";
        public const string DisposeInternalKey = "DisposeInternalKey";
        public const string DisposeKey = "DisposeKey";
        public const string OnDisposedKey = "OnDisposedKey";

        #endregion

        #region Test methods

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeNone()
        {
            const ObservationMode mode = ObservationMode.None;
            ViewModelBase parentViewModel = GetViewModelBase();

            var viewModel = parentViewModel.GetViewModel<TestViewModelBase>(observationMode: mode);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeFalse();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeParentObserveChild()
        {
            const ObservationMode mode = ObservationMode.ParentObserveChild;
            ViewModelBase parentViewModel = GetViewModelBase();

            var viewModel = parentViewModel.GetViewModel<TestViewModelBase>(observationMode: mode);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeChildObserveParent()
        {
            const ObservationMode mode = ObservationMode.ChildObserveParent;
            ViewModelBase parentViewModel = GetViewModelBase();

            var viewModel = parentViewModel.GetViewModel<TestViewModelBase>(observationMode: mode);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeFalse();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeBoth()
        {
            const ObservationMode mode = ObservationMode.Both;
            ViewModelBase parentViewModel = GetViewModelBase();

            var viewModel = parentViewModel.GetViewModel<TestViewModelBase>(observationMode: mode);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromApplicationSettingsNotSpecifiedExplicitly()
        {
            ApplicationSettings.ViewModelObservationMode = ObservationMode.Both;
            ViewModelBase parentViewModel = GetViewModelBase();

            var viewModel = parentViewModel.GetViewModel<TestViewModelBase>();
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void DefaultBusyMessageShouldEqualsToSettingsDefaultBusyMessage()
        {
            const string busyMessage = "busy...";
            var settings = new DefaultViewModelSettings { DefaultBusyMessage = busyMessage };
            ServiceProvider.ViewModelSettingsFactory = model => settings;

            ViewModelBase viewModel = GetViewModelBase();

            viewModel.BeginBusy();
            viewModel.BusyMessage.ShouldEqual(busyMessage);
        }

        [TestMethod]
        public void SeveralBusyMessagesShouldBeDisplayedInOrderOfRegistration()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            var firstId = viewModel.BeginBusy(firstBusyMessage);
            var secondId = viewModel.BeginBusy(secondBusyMessage);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            secondId.Dispose();

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(firstBusyMessage);
            firstId.Dispose();

            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void HandleBusyMessageShouldNotUpdateMessageIfModeNone()
        {
            const string busyMessageString = "busy...";
            var viewModel = GetViewModelBase();
            viewModel.Settings.HandleBusyMessageMode = HandleMode.None;
            var busyMessage = new BusyTokenMock(busyMessageString);
            IHandler<object> beginBusyHandler = viewModel;

            beginBusyHandler.Handle(this, busyMessage);
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void HandleBusyMessageShouldUpdateMessageIfModeHandle()
        {
            const string busyMessageString = "busy...";
            var viewModel = GetViewModelBase();
            viewModel.Settings.HandleBusyMessageMode = HandleMode.Handle;
            var busyMessage = new BusyTokenMock(busyMessageString);
            IHandler<object> beginBusyHandler = viewModel;

            beginBusyHandler.Handle(this, busyMessage);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);

            busyMessage.Dispose();
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void HandleBusyMessageShouldUpdateMessageAndNotifyListinersIfModeHandleAndNotifyListeners()
        {
            const string busyMessageString = "busy...";
            var viewModel = GetViewModelBase();
            var childViewModel = viewModel.GetViewModel<TestViewModelBase>(observationMode: ObservationMode.None);
            viewModel.Subscribe(childViewModel);
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifySubscribers;
            var busyMessage = new BusyTokenMock(busyMessageString);
            IHandler<object> beginBusyHandler = viewModel;

            beginBusyHandler.Handle(this, busyMessage);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);
            childViewModel.IsBusy.ShouldBeTrue();
            childViewModel.BusyMessage.ShouldEqual(busyMessageString);

            busyMessage.Dispose();
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
            childViewModel.IsBusy.ShouldBeFalse();
            childViewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void BeginBusyShouldNotifyListiners()
        {
            const string busyMessageString = "busy...";
            var viewModel = GetViewModelBase();
            var childViewModel = viewModel.GetViewModel<TestViewModelBase>(observationMode: ObservationMode.None);
            viewModel.Subscribe(childViewModel);
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifySubscribers;

            var beginBusy = viewModel.BeginBusy(busyMessageString);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);
            childViewModel.IsBusy.ShouldBeTrue();
            childViewModel.BusyMessage.ShouldEqual(busyMessageString);

            beginBusy.Dispose();
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
            childViewModel.IsBusy.ShouldBeFalse();
            childViewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void GetBusyTokensShouldReturnAllBusyMessages()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            viewModel.GetBusyTokens().ShouldBeEmpty();

            viewModel.BeginBusy(firstBusyMessage);
            viewModel.GetBusyTokens().Single().Message.ShouldEqual(firstBusyMessage);

            viewModel.BeginBusy(secondBusyMessage);
            var tokens = viewModel.GetBusyTokens();
            tokens.Count.ShouldEqual(2);
            tokens[1].Message.ShouldEqual(secondBusyMessage);

            foreach (var token in tokens)
                token.Dispose();
            viewModel.GetBusyTokens().ShouldBeEmpty();
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void ClearBusyShouldRemoveAllBusyMessages()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            viewModel.BeginBusy(firstBusyMessage);
            viewModel.BeginBusy(secondBusyMessage);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            viewModel.ClearBusy();

            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void ClearBusyShouldNotifyListiners()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            var childViewModel = viewModel.GetViewModel<TestViewModelBase>(observationMode: ObservationMode.None);
            viewModel.Subscribe(childViewModel);
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifySubscribers;

            viewModel.BeginBusy(firstBusyMessage);
            viewModel.BeginBusy(secondBusyMessage);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            childViewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            childViewModel.IsBusy.ShouldBeTrue();
            viewModel.ClearBusy();

            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
            childViewModel.IsBusy.ShouldBeFalse();
            childViewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void WhenVmAddedToListenersItShouldGetAllBusyMessage()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            viewModel.BeginBusy(firstBusyMessage);
            viewModel.BeginBusy(secondBusyMessage);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);

            var testViewModelBase = viewModel.GetViewModel<TestViewModelBase>(observationMode: ObservationMode.None);
            viewModel.Subscribe(testViewModelBase);

            testViewModelBase.IsBusy.ShouldBeTrue();
            testViewModelBase.BusyMessage.ShouldEqual(secondBusyMessage);
        }

        [TestMethod]
        public void VmShouldDisposeInnerCommandIfSettingsTrue()
        {
            ViewModelBase viewModel = GetViewModelBase();
            var viewModelBaseWithCommand = viewModel.GetViewModel<ViewModelBaseWithCommand>();
            viewModelBaseWithCommand.Settings.DisposeCommands = true;
            var withoutGetter = new RelayCommandMock();
            var command = new RelayCommandMock();

            viewModelBaseWithCommand.NullCommand = null;
            viewModelBaseWithCommand.Command = command;
            viewModelBaseWithCommand.CommandWithoutGetter = withoutGetter;
            viewModelBaseWithCommand.Dispose();
            command.IsDisposed.ShouldBeTrue();
            withoutGetter.IsDisposed.ShouldBeTrue();
            viewModelBaseWithCommand.IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void VmShouldNotDisposeInnerCommandIfSettingsFalse()
        {
            ViewModelBase viewModel = GetViewModelBase();
            var viewModelBaseWithCommand = viewModel.GetViewModel<ViewModelBaseWithCommand>();
            viewModelBaseWithCommand.Settings.DisposeCommands = false;
            var withoutGetter = new RelayCommandMock();
            var command = new RelayCommandMock();

            viewModelBaseWithCommand.NullCommand = null;
            viewModelBaseWithCommand.Command = command;
            viewModelBaseWithCommand.CommandWithoutGetter = withoutGetter;
            viewModelBaseWithCommand.Dispose();
            command.IsDisposed.ShouldBeFalse();
            withoutGetter.IsDisposed.ShouldBeFalse();
            viewModelBaseWithCommand.IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void VmShouldClearListenersWhenDisposed()
        {
            ViewModelBase viewModel = GetViewModelBase();
            var spyHandler = new SpyHandler();
            viewModel.Subscribe(spyHandler);
            viewModel.LocalEventAggregator.GetObservers().Contains(spyHandler).ShouldBeTrue();
            viewModel.Dispose();
            viewModel.LocalEventAggregator.GetObservers().Contains(spyHandler).ShouldBeFalse();
            viewModel.IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void VmShouldThrowsExceptionDisposed()
        {
            IViewModel viewModel = GetViewModelBase();
            viewModel.Dispose();
            viewModel.IsDisposed.ShouldBeTrue();

            ShouldThrow<ObjectDisposedException>(() => viewModel.InitializeViewModel(DataContext.Empty));
        }

        [TestMethod]
        public void VmShouldCallDisposedOnce1()
        {
            int count = 0;
            ViewModelBase viewModel = GetViewModelBase();
            viewModel.IsInitialized.ShouldBeTrue();
            viewModel.IsDisposed.ShouldBeFalse();
            viewModel.IsRestored.ShouldBeFalse();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.IsDisposed.ShouldBeTrue();
            viewModel.IsInitialized.ShouldBeTrue();
            viewModel.IsRestored.ShouldBeFalse();
            count.ShouldEqual(1);
        }

        [TestMethod]
        public void VmShouldCallDisposedOnce2()
        {
            int count = 0;
            ViewModelBase viewModel = new ViewModelBaseWithDisplayName();
            viewModel.IsInitialized.ShouldBeFalse();
            viewModel.IsDisposed.ShouldBeFalse();
            viewModel.IsRestored.ShouldBeFalse();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.IsDisposed.ShouldBeTrue();
            viewModel.IsInitialized.ShouldBeTrue();
            viewModel.IsRestored.ShouldBeFalse();
            count.ShouldEqual(1);
        }

        [TestMethod]
        public void VmShouldCallDisposedOnce3()
        {
            int count = 0;
            ViewModelBase viewModel = GetViewModelBase(new DataContext(InitializationConstants.IsRestored.ToValue(true)));
            viewModel.IsDisposed.ShouldBeFalse();
            viewModel.IsInitialized.ShouldBeTrue();
            viewModel.IsRestored.ShouldBeTrue();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.Disposed += (sender, args) => ++count;
            viewModel.Dispose();

            viewModel.IsDisposed.ShouldBeTrue();
            viewModel.IsInitialized.ShouldBeTrue();
            viewModel.IsRestored.ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [TestMethod]
        public void VmShouldCallMethodsInSpecificOrder()
        {
            ViewModelBase viewModel = GetViewModelBase();
            var orderViewModelBase = viewModel.GetViewModel<OrderViewModelBase>();
            orderViewModelBase.Dispose();

            orderViewModelBase.MethodCallCollection.Count.ShouldEqual(6);
            orderViewModelBase.MethodCallCollection[0].ShouldEqual(InititalizingViewModelKey);
            orderViewModelBase.MethodCallCollection[1].ShouldEqual(InitViewModelInternalKey);
            orderViewModelBase.MethodCallCollection[2].ShouldEqual(InitViewModelKey);
            orderViewModelBase.MethodCallCollection[3].ShouldEqual(DisposeInternalKey);
            orderViewModelBase.MethodCallCollection[4].ShouldEqual(DisposeKey);
            orderViewModelBase.MethodCallCollection[5].ShouldEqual(OnDisposedKey);
        }

        #endregion
    }
}
