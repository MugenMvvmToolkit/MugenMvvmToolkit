using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public partial class ViewModelBaseTest : TestBase
    {
        #region Nested types

        public class ViewModelBaseWithContext : ViewModelBase, IViewModel
        {
            #region Fields

            private List<Action> _actions = new List<Action>();

            #endregion

            #region Properties

            public IDataContext Context { get; private set; }

            #endregion

            #region Implementation of IViewModel

            /// <summary>
            ///     Initializes the current view model with specified <see cref="IDataContext" />.
            /// </summary>
            /// <param name="context">
            ///     The specified <see cref="IDataContext" />.
            /// </param>
            public void InitializeViewModel(IDataContext context)
            {
                Context = context;
                foreach (Action action in _actions)
                {
                    action();
                }
                _actions = null;
            }

            #endregion
        }

        public class TestViewModelBase : ViewModelBase
        {
            public IThreadManager ThreadManager
            {
                get { return base.ThreadManager; }
            }
        }

        public class ViewModelBaseWithDisplayName : ViewModelBase, IHasDisplayName
        {
            #region Implementation of IHasDisplayName

            /// <summary>
            ///     Gets or sets the display name for the current model.
            /// </summary>
            public string DisplayName { get; set; }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void GetViewModelMethodShouldUseCorrectParameters()
        {
            const IocContainerCreationMode iocContainerCreationMode = IocContainerCreationMode.ParentViewModel;
            const ObservationMode listenType = ObservationMode.Both;
            var constantValue = DataConstantValue.Create(new DataConstant<string>("test"), "test");
            bool isInvoked = false;

            var providerMock = new ViewModelProviderMock();
            ServiceProvider.ViewModelProvider = providerMock;
            var func = IocContainer.GetFunc;
            IocContainer.GetFunc = (type, s, arg3) =>
            {
                if (type == typeof(IViewModelProvider))
                    return providerMock;
                return func(type, s, arg3);
            };
            ViewModelBase viewModel = GetViewModelBase();
            Action<IDataContext> check = context =>
            {
                context.GetData((DataConstant<string>)constantValue.DataConstant).ShouldEqual(constantValue.Value);
                context.GetDataTest(InitializationConstants.ObservationMode).ShouldEqual(listenType);
                context.GetDataTest(InitializationConstants.IocContainerCreationMode).ShouldEqual(iocContainerCreationMode);
                isInvoked = true;
            };
            providerMock.GetViewModel = (@delegate, context) =>
            {
                check(context);
                return @delegate(IocContainer);
            };
            providerMock.GetViewModelType = (type, context) =>
            {
                check(context);
                return new ViewModelBaseWithContext();
            };


            viewModel.GetViewModel(getViewModel: adapter => new ViewModelBaseWithContext(),
                containerCreationMode: iocContainerCreationMode, observationMode: listenType, parameters: constantValue);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            viewModel.GetViewModel(adapter => new ViewModelBaseWithContext(), listenType, iocContainerCreationMode, constantValue);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            viewModel.GetViewModel(typeof(ViewModelBaseWithContext), listenType, iocContainerCreationMode, constantValue);
            isInvoked.ShouldBeTrue();
            isInvoked = false;

            viewModel.GetViewModel<ViewModelBaseWithContext>(listenType, iocContainerCreationMode, constantValue);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void WhenVmIsInitializedPropertyIsInitializedShouldChanged()
        {
            var testViewModel = new TestViewModelBase();
            testViewModel.IsInitialized.ShouldBeFalse();
            ViewModelProvider.InitializeViewModel(testViewModel, null);
            testViewModel.IsInitialized.ShouldBeTrue();
        }

        [TestMethod]
        public void ReInitializationShouldNotThrowExceptionThrowOnMultiInitFalse()
        {
            ThreadManager.ImmediateInvokeAsync = true;
            ViewModelBase viewModel = GetViewModelBase();
            ViewModelProvider.InitializeViewModel(viewModel, null);
        }

        [TestMethod]
        public void DefaultPropertiesShouldBeInitialized()
        {
            ThreadManager.ImmediateInvokeAsync = true;
            Settings.WithoutClone = true;
            ViewModelBase viewModel = GetViewModelBase();

            var testViewModel = viewModel.GetViewModel<TestViewModelBase>();
            testViewModel.ThreadManager.ShouldEqual(ThreadManager);
            testViewModel.IocContainer.Parent.ShouldEqual(viewModel.IocContainer);
            testViewModel.Settings.ShouldEqual(Settings);
        }

        [TestMethod]
        public void DisplayNameShouldBeInitializedIfProviderCanBeResolved()
        {
            const string name = "name";
            ThreadManager.ImmediateInvokeAsync = true;
            Settings.WithoutClone = true;
            DisplayNameProvider.GetNameDelegate = info => name;
            ViewModelBase viewModel = GetViewModelBase();

            var testViewModel = viewModel.GetViewModel<ViewModelBaseWithDisplayName>();
            testViewModel.DisplayName.ShouldEqual(name);
        }

        [TestMethod]
        public void DisplayNameShouldNotBeInitializedIfProviderCannotBeResolved()
        {
            ThreadManager.ImmediateInvokeAsync = true;
            CanBeResolvedTypes.Remove(typeof(IDisplayNameProvider));
            ViewModelBase viewModel = GetViewModelBase();

            var testViewModel = viewModel.GetViewModel<ViewModelBaseWithDisplayName>();
            testViewModel.DisplayName.ShouldBeNull();
        }

        [TestMethod]
        public void InitializedEventShouldBeInvokedAfterViewModelWasInitialized()
        {
            var testViewModel = new TestViewModelBase();
            testViewModel.IsInitialized.ShouldBeFalse();
            bool isInvoked = false;
            Action invokeAction = () => isInvoked = true;
            testViewModel.Initialized += (sender, args) => invokeAction();
            isInvoked.ShouldBeFalse();

            ViewModelProvider.InitializeViewModel(testViewModel, null);
            testViewModel.IsInitialized.ShouldBeTrue();
            isInvoked.ShouldBeTrue();
        }

        #endregion

        #region Properties

        protected Func<ViewModelBase> GetViewModelBaseDelegate { get; set; }

        #endregion

        #region Methods

        private ViewModelBase GetViewModelBase()
        {
            ViewModelBase viewModel = GetViewModelBaseDelegate();
            InitializeViewModel(viewModel, IocContainer);
            return viewModel;
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            GetViewModelBaseDelegate = () => new TestViewModelBase();
        }

        #endregion
    }
}