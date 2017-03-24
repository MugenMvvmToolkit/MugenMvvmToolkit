#region Copyright

// ****************************************************************************
// <copyright file="ViewModelProviderTest.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class ViewModelProviderTest : TestBase
    {
        #region Nested types

        protected class ViewModel : ViewModelBase, IViewModel, IHasState, IParentAwareViewModel
        {
            #region Properties

            public Action<IDataContext> InitializeViewModel { get; set; }

            public static Action<IDataContext> InitializeViewModelStatic { get; set; }

            public int LoadStateCount { get; set; }

            public IDataContext LoadStateContext { get; set; }

            public int SaveStateCount { get; set; }

            public IDataContext SaveStateContext { get; set; }

            public IViewModel Parent { get; set; }

            #endregion

            #region Implementation of IViewModel

            public IIocContainer IocContainer
            {
                get { return base.IocContainer; }
                set { base.IocContainer = value; }
            }

            void IViewModel.InitializeViewModel(IDataContext context)
            {
                if (InitializeViewModel != null)
                    InitializeViewModel(context);
                IocContainer = context.GetData(InitializationConstants.IocContainer);
                if (InitializeViewModelStatic != null)
                    InitializeViewModelStatic(context);
            }

            public void SetParent(IViewModel parent)
            {
                Parent = parent;
            }

            #endregion

            #region Implementation of IHasState

            void IHasState.LoadState(IDataContext state)
            {
                ++LoadStateCount;
                LoadStateContext = state;
            }

            void IHasState.SaveState(IDataContext state)
            {
                ++SaveStateCount;
                SaveStateContext = state;
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void GetViewModelShouldUseDelegateToCreateViewModel()
        {
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var context = new DataContext();

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                    {
                        dataContext.ShouldEqual(context);
                        ++initialize;
                    }
            };

            IViewModel viewModel = provider.GetViewModel(container => vm, context);
            viewModel.ShouldEqual(vm);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void GetViewModelShouldUseIocContainerToCreateViewModel()
        {
            var context = new DataContext();
            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.ShouldEqual(context);
                    ++initialize;
                }
            };
            var iocContainer = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return vm;
                }
            };

            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            IViewModel viewModel = provider.GetViewModel(typeof(ViewModel), context);
            viewModel.ShouldEqual(vm);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void GetViewModelShouldUseIocContainerToCreateViewModelWithParameters()
        {
            var parameters = new IIocParameter[0];
            const string vmName = "vmName";
            int initialize = 0;

            var context = new DataContext
            {
                {InitializationConstants.ViewModelBindingName, vmName},
                {InitializationConstants.IocParameters, parameters}
            };
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.ShouldEqual(context);
                    ++initialize;
                }
            };
            var iocContainer = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    s.ShouldEqual(vmName);
                    arg3.ShouldEqual(parameters);
                    type.ShouldEqual(typeof(ViewModel));
                    return vm;
                }
            };

            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            IViewModel viewModel = provider.GetViewModel(typeof(ViewModel), context);
            viewModel.ShouldEqual(vm);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void InitializeViewModelShouldInitializeViewModel()
        {
            var iocContainer = new IocContainerMock();
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext();

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.ShouldEqual(context);
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(2);
        }

        [TestMethod]
        public void ProviderShouldUseObservationModeFromDataContextModeNone()
        {
            const ObservationMode mode = ObservationMode.None;
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var parentViewModel = new ViewModel();
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, mode},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var viewModel = (ViewModel)provider.GetViewModel(container => new ViewModel(), context);
            viewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldUseObservationModeFromDataContextModeParentObserveChild()
        {
            const ObservationMode mode = ObservationMode.ParentObserveChild;
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var parentViewModel = new ViewModel();
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, mode},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var viewModel = (ViewModel)provider.GetViewModel(container => new ViewModel(), context);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeChildObserveParent()
        {
            const ObservationMode mode = ObservationMode.ChildObserveParent;
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var parentViewModel = new ViewModel();
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, mode},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var viewModel = (ViewModel)provider.GetViewModel(container => new ViewModel(), context);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeFalse();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromDataContextModeBoth()
        {
            const ObservationMode mode = ObservationMode.Both;
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var parentViewModel = new ViewModel();
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, mode},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var viewModel = (ViewModel)provider.GetViewModel(container => new ViewModel(), context);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void WhenInitialazingVmShouldUseObservationModeFromApplicationSettingsNotSpecifiedExplicitly()
        {
            ApplicationSettings.ViewModelObservationMode = ObservationMode.Both;
            var iocContainer = new IocContainerMock();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            var parentViewModel = new ViewModel();
            var context = new DataContext
            {
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var viewModel = (ViewModel)provider.GetViewModel(container => new ViewModel(), context);
            parentViewModel.LocalEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldCallSaveStateMethodOnPreserveViewModel()
        {
            var iocContainer = new IocContainerMock();
            var viewModel = new ViewModel();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);

            provider.PreserveViewModel(viewModel, DataContext.Empty);
            viewModel.SaveStateCount.ShouldEqual(1);
            viewModel.LoadStateCount.ShouldEqual(0);
        }

        [TestMethod]
        public void ProviderShouldCallLoadStateMethodOnRestoreViewModel()
        {
            var loadViewModel = new ViewModel();
            var iocContainer = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    typeof(ViewModel).ShouldEqual(type);
                    return loadViewModel;
                }
            };
            var viewModel = new ViewModel();
            ViewModelProvider provider = GetViewModelProvider(iocContainer);

            var state = provider.PreserveViewModel(viewModel, DataContext.Empty);
            var restoreViewModel = provider.RestoreViewModel(state, DataContext.Empty, true);
            restoreViewModel.ShouldEqual(loadViewModel);
            loadViewModel.LoadStateCount.ShouldEqual(1);
            loadViewModel.LoadStateContext.ShouldEqual(state);
        }

        [TestMethod]
        public void ProviderShouldThrowExceptionInvalidStateThrowOnErrorTrue()
        {
            var provider = GetViewModelProvider(new IocContainerMock());
            ShouldThrow(() => provider.RestoreViewModel(DataContext.Empty, DataContext.Empty, true));
        }

        [TestMethod]
        public void ProviderShouldNotThrowExceptionInvalidStateThrowOnErrorFalse()
        {
            var provider = GetViewModelProvider(new IocContainerMock());
            provider.RestoreViewModel(DataContext.Empty, DataContext.Empty, false).ShouldBeNull();
        }

        [TestMethod]
        public void ProviderShouldRestorePreserveViewModelState()
        {
            bool restoringCalled = false;
            bool restoredCalled = false;
            bool preservingCalled = false;
            bool preservedCalled = false;
            var preserveCtx = new DataContext();
            var restoreCtx = new DataContext();
            DataConstant<string> key = "key";
            const string value = "value";
            var loadViewModel = new ViewModel();

            IocContainer.GetFunc = (type, s, arg3) =>
            {
                typeof(ViewModel).ShouldEqual(type);
                return loadViewModel;
            };
            var viewModel = new ViewModel();
            viewModel.Settings.State.Add(key, value);
            ViewModelProvider provider = GetViewModelProvider(IocContainer);

            provider.Preserving += (sender, args) =>
            {
                sender.ShouldEqual(provider);
                args.Context.ShouldEqual(preserveCtx);
                args.ViewModel.ShouldEqual(viewModel);
                preservingCalled = true;
            };
            ViewModelPreservedEventArgs preservedEventArgs = null;
            provider.Preserved += (sender, args) =>
            {
                sender.ShouldEqual(provider);
                args.Context.ShouldEqual(preserveCtx);
                args.ViewModel.ShouldEqual(viewModel);
                preservedEventArgs = args;
                preservedCalled = true;
            };

            var state = provider.PreserveViewModel(viewModel, preserveCtx);
            state.ShouldEqual(preservedEventArgs.State);
            provider.Restoring += (sender, args) =>
            {
                sender.ShouldEqual(provider);
                args.Context.ShouldEqual(restoreCtx);
                args.State.ShouldEqual(state);
                restoringCalled = true;
            };
            ViewModelRestoredEventArgs restoredEventArgs = null;
            provider.Restored += (sender, args) =>
            {
                sender.ShouldEqual(provider);
                args.Context.ShouldEqual(restoreCtx);
                args.State.ShouldEqual(state);
                restoredEventArgs = args;
                restoredCalled = true;
            };

            var restoreViewModel = provider.RestoreViewModel(state, restoreCtx, true);
            restoreViewModel.ShouldEqual(restoredEventArgs.ViewModel);
            restoreViewModel.IocContainer.ShouldEqual(IocContainer);
            restoreViewModel.ShouldEqual(loadViewModel);
            restoreViewModel.Settings.State.GetData(key).ShouldEqual(value);
            restoringCalled.ShouldBeTrue();
            restoredCalled.ShouldBeTrue();
            preservingCalled.ShouldBeTrue();
            preservedCalled.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModel()
        {
            IocContainer.GetFunc = (type, s, arg3) =>
            {
                type.ShouldEqual(typeof(ViewModel));
                return new ViewModel();
            };

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(IocContainer);
            var context = new DataContext
            {
                {InitializationConstants.ParentViewModel, parentViewModel}
            };
            var restoreContext = new DataContext
            {
                {InitializationConstants.IgnoreViewModelCache, true}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, restoreContext, true);
            vm = (ViewModel)provider.RestoreViewModel(state, restoreContext, true);

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            vm.Parent.ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(IocContainer);
            vm.IocContainer.ShouldEqual(IocContainer);
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModelAfterChildViewModelRestore()
        {
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                }
            };

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { GetFunc = parentIoc.GetFunc });
            var context = new DataContext
            {
                {InitializationConstants.ParentViewModel, parentViewModel}
            };
            var restoreContext = new DataContext
            {
                {InitializationConstants.IgnoreViewModelCache, true}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            vm = (ViewModel)provider.RestoreViewModel(state, restoreContext, true);
            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, restoreContext, true);
            parentViewModel.IocContainer = parentIoc;

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            vm.Parent.ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(parentIoc);
            vm.IocContainer.ShouldEqual(parentIoc);
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModelRestoreChildDuringRestoration()
        {
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                }
            };

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { GetFunc = parentIoc.GetFunc });
            var context = new DataContext
            {
                {InitializationConstants.ParentViewModel, parentViewModel}
            };
            var restoreContext = new DataContext
            {
                {InitializationConstants.IgnoreViewModelCache, true}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            ViewModel.InitializeViewModelStatic = dataContext =>
            {
                ViewModel.InitializeViewModelStatic = null;
                vm = (ViewModel)provider.RestoreViewModel(state, restoreContext, true);
            };
            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, restoreContext, true);
            parentViewModel.IocContainer = parentIoc;

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            vm.Parent.ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(parentIoc);
            vm.IocContainer.ShouldEqual(parentIoc);
        }

        [TestMethod]
        public void ProviderShouldRestoreObservationMode()
        {
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                }
            };

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { GetFunc = parentIoc.GetFunc });
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, ObservationMode.Both},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };
            var restoreContext = new DataContext
            {
                {InitializationConstants.IgnoreViewModelCache, true}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, restoreContext, true);
            vm = (ViewModel)provider.RestoreViewModel(state, restoreContext, true);

            vm.LocalEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
            parentViewModel.LocalEventAggregator.GetObservers().Contains(vm).ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldCacheRestoredViewModels()
        {
            var loadViewModel = new ViewModel();
            var childIoc = new IocContainerMock();
            var iocContainer = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    typeof(ViewModel).ShouldEqual(type);
                    return loadViewModel;
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = iocContainer.GetFunc;
            ViewModelProvider provider = GetViewModelProvider(iocContainer);

            var state = provider.PreserveViewModel(new ViewModel(), DataContext.Empty);
            var restoreViewModel1 = provider.RestoreViewModel(state, DataContext.Empty, true);
            restoreViewModel1.ShouldEqual(loadViewModel);

            loadViewModel = new ViewModel();

            var restoreViewModel2 = provider.RestoreViewModel(state, DataContext.Empty, true);
            restoreViewModel2.ShouldEqual(restoreViewModel1);

            //No cache
            var ctx = new DataContext { { InitializationConstants.IgnoreViewModelCache, true } };
            var restoreViewModel3 = provider.RestoreViewModel(state, ctx, true);
            restoreViewModel3.ShouldEqual(loadViewModel);

            //Dispose current view model.
            restoreViewModel1.Dispose();
            var restoreViewModel4 = provider.RestoreViewModel(state, DataContext.Empty, true);
            restoreViewModel4.ShouldEqual(loadViewModel);
        }

        [TestMethod]
        public void ProviderShouldRestoreViewModelFromType()
        {
            var loadViewModel = new ViewModel();
            var childIoc = new IocContainerMock();
            var iocContainer = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    typeof(ViewModel).ShouldEqual(type);
                    return loadViewModel;
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = iocContainer.GetFunc;
            ViewModelProvider provider = GetViewModelProvider(iocContainer);

            var context = new DataContext { { InitializationConstants.ViewModelType, typeof(ViewModel) } };
            provider.RestoreViewModel(DataContext.Empty, context, true).ShouldEqual(loadViewModel);
        }

        protected virtual ViewModelProvider GetViewModelProvider(IIocContainer iocContainer)
        {
            return new ViewModelProvider(iocContainer);
        }

        #endregion
    }
}
