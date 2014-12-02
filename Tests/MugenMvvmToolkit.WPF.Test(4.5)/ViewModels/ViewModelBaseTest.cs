using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            /// <summary>
            ///     Occurs after the view model is fully loaded.
            /// </summary>
            internal override void OnInitializedInternal()
            {
                MethodCallCollection.Add(InitViewModelInternalKey);
                base.OnInitializedInternal();
            }

            /// <summary>
            ///     Occurs during the initialization of the view model.
            /// </summary>
            protected override void OnInitializing(IDataContext context)
            {
                MethodCallCollection.Add(InititalizingViewModelKey);
                base.OnInitializing(context);
            }

            /// <summary>
            ///     Occurs after the view model is fully loaded.
            /// </summary>
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

            /// <summary>
            ///     Occurs after the current view model is disposed, use for clear resource and event listeners(Internal only).
            /// </summary>
            internal override void OnDisposeInternal(bool disposing)
            {
                MethodCallCollection.Add(DisposeInternalKey);
                base.OnDisposeInternal(disposing);
            }

            /// <summary>
            ///     Occurs after the current view model is disposed, use for clear resource and event listeners.
            /// </summary>
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
            Settings.WithoutClone = true;
            Settings.DefaultBusyMessage = busyMessage;
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
            viewModel.EndBusy(secondId);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(firstBusyMessage);
            viewModel.EndBusy(firstId);

            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void HandleBusyMessageShouldNotUpdateMessageIfModeNone()
        {
            const string busyMessageString = "busy...";
            var viewModel = GetViewModelBase();
            viewModel.Settings.HandleBusyMessageMode = HandleMode.None;
            var busyMessage = new BeginBusyMessage(Guid.NewGuid(), busyMessageString);
            IHandler<BeginBusyMessage> beginBusyHandler = viewModel;

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
            var busyMessage = new BeginBusyMessage(Guid.NewGuid(), busyMessageString);
            IHandler<BeginBusyMessage> beginBusyHandler = viewModel;
            IHandler<EndBusyMessage> endBusyHandler = viewModel;

            beginBusyHandler.Handle(this, busyMessage);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);

            endBusyHandler.Handle(this, busyMessage.ToEndBusyMessage());
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
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifyObservers;
            var busyMessage = new BeginBusyMessage(Guid.NewGuid(), busyMessageString);
            IHandler<BeginBusyMessage> beginBusyHandler = viewModel;
            IHandler<EndBusyMessage> endBusyHandler = viewModel;

            beginBusyHandler.Handle(this, busyMessage);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);
            childViewModel.IsBusy.ShouldBeTrue();
            childViewModel.BusyMessage.ShouldEqual(busyMessageString);

            endBusyHandler.Handle(this, busyMessage.ToEndBusyMessage());
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
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifyObservers;

            var beginBusy = viewModel.BeginBusy(busyMessageString);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(busyMessageString);
            childViewModel.IsBusy.ShouldBeTrue();
            childViewModel.BusyMessage.ShouldEqual(busyMessageString);

            viewModel.EndBusy(beginBusy);
            viewModel.IsBusy.ShouldBeFalse();
            viewModel.BusyMessage.ShouldBeNull();
            childViewModel.IsBusy.ShouldBeFalse();
            childViewModel.BusyMessage.ShouldBeNull();
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
            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifyObservers;

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
        public void WhenVmRemovedFromListenersItShouldRemoveAllBusyMessageFromVm()
        {
            const string firstBusyMessage = "busy1...";
            const string secondBusyMessage = "busy2...";

            ViewModelBase viewModel = GetViewModelBase();
            var childViewModel = viewModel.GetViewModel<TestViewModelBase>(observationMode: ObservationMode.None);
            viewModel.Subscribe(childViewModel);

            viewModel.Settings.HandleBusyMessageMode = HandleMode.HandleAndNotifyObservers;

            viewModel.BeginBusy(firstBusyMessage);
            viewModel.BeginBusy(secondBusyMessage);

            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            childViewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            childViewModel.IsBusy.ShouldBeTrue();

            viewModel.Unsubscribe(childViewModel);
            viewModel.IsBusy.ShouldBeTrue();
            viewModel.BusyMessage.ShouldEqual(secondBusyMessage);
            childViewModel.IsBusy.ShouldBeFalse();
            childViewModel.BusyMessage.ShouldBeNull();
        }

        [TestMethod]
        public void VmShouldDisposeIocContainerIfSettingsTrue()
        {
            IViewModel viewModel = GetViewModelBase();
            viewModel.Settings.DisposeIocContainer = true;
            viewModel.Dispose();
            ((IocContainerMock)viewModel.IocContainer).IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void VmShouldNotDisposeIocContainerIfSettingsFalse()
        {
            IViewModel viewModel = GetViewModelBase();
            viewModel.Settings.DisposeIocContainer = false;
            viewModel.Dispose();
            ((IocContainerMock)viewModel.IocContainer).IsDisposed.ShouldBeFalse();
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
        }

        [TestMethod]
        public void VmShouldThrowsExceptionDisposed()
        {
            ViewModelBase viewModel = GetViewModelBase();
            viewModel.Dispose();

            ShouldThrow<ObjectDisposedException>(() => viewModel.GetViewModel<TestViewModelBase>());
            ShouldThrow<ObjectDisposedException>(() => viewModel.GetViewModel(typeof(TestViewModelBase)));
            ShouldThrow<ObjectDisposedException>(() => viewModel.GetViewModel(adapter => new TestViewModelBase()));
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