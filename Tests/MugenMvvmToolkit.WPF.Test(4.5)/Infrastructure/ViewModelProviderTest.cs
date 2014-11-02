using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class ViewModelProviderTest : TestBase
    {
        #region Nested types

        protected class ViewModel : ViewModelBase, IViewModel, IHasState
        {
            #region Properties

            public Action<IDataContext> InitializeViewModel { get; set; }

            public static Action<IDataContext> InitializeViewModelStatic { get; set; }

            public int LoadStateCount { get; set; }

            public IDataContext LoadStateContext { get; set; }

            public int SaveStateCount { get; set; }

            public IDataContext SaveStateContext { get; set; }

            #endregion

            #region Implementation of IViewModel

            public new IIocContainer IocContainer { get; set; }

            void IViewModel.InitializeViewModel(IDataContext context)
            {
                if (InitializeViewModel != null)
                    InitializeViewModel(context);
                IocContainer = context.GetData(InitializationConstants.IocContainer);
                if (InitializeViewModelStatic != null)
                    InitializeViewModelStatic(context);
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
        public void ProviderShouldCreateIocContainerDefaultMode()
        {
            var child = new IocContainerMock();
            var iocContainer = new IocContainerMock
            {
                CreateChild = mock => child
            };
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Default}
            };

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.GetData(InitializationConstants.IocContainer).ShouldEqual(child);
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void ProviderShouldCreateIocContainerParentViewModelModeWithoutParentViewModel()
        {
            var child = new IocContainerMock();
            var iocContainer = new IocContainerMock
            {
                CreateChild = mock => child
            };
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.ParentViewModel}
            };

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.GetData(InitializationConstants.IocContainer).ShouldEqual(child);
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void ProviderShouldCreateIocContainerParentViewModelMode()
        {
            var iocContainer = new IocContainerMock();
            var viewModel = new ViewModel
            {
                IocContainer = new IocContainerMock
                {
                    CreateChild = mock => iocContainer
                }
            };
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.ParentViewModel},
                {InitializationConstants.ParentViewModel, viewModel}
            };

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.GetData(InitializationConstants.IocContainer).ShouldEqual(iocContainer);
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void ProviderShouldCreateIocContainerMixedMode()
        {
            var parentViewModelIoc = new IocContainerMock();
            var child = new IocContainerMock();
            parentViewModelIoc.GetFunc = (type, s, arg3) => parentViewModelIoc;
            child.GetFunc = (type, s, arg3) => child;

            var iocContainer = new IocContainerMock
            {
                CreateChild = mock => child
            };
            var viewModel = new ViewModel
            {
                IocContainer = new IocContainerMock
                {
                    CreateChild = mock => parentViewModelIoc
                }
            };
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Mixed},
                {InitializationConstants.ParentViewModel, viewModel}
            };

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(1);

            bool disposed = false;
            vm.IocContainer.Disposed += (sender, args) => disposed = true;

            vm.IocContainer.IsDisposed.ShouldBeFalse();
            vm.IocContainer.Get(typeof(object)).ShouldEqual(parentViewModelIoc);
            parentViewModelIoc.Dispose();

            disposed.ShouldBeFalse();
            vm.IocContainer.IsDisposed.ShouldBeFalse();
            vm.IocContainer.Get(typeof(object)).ShouldEqual(child);

            child.Dispose();
            disposed.ShouldBeTrue();
            vm.IocContainer.IsDisposed.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldCreateIocContainerMixedModeWithoutParentViewModel()
        {
            var child = new IocContainerMock();
            var iocContainer = new IocContainerMock
            {
                CreateChild = mock => child
            };
            var provider = GetViewModelProvider(iocContainer);
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Mixed}
            };

            int initialize = 0;
            var vm = new ViewModel
            {
                InitializeViewModel = dataContext =>
                {
                    dataContext.GetData(InitializationConstants.IocContainer).ShouldEqual(child);
                    ++initialize;
                }
            };
            provider.InitializeViewModel(vm, context);
            initialize.ShouldEqual(1);
        }

        [TestMethod]
        public void ProviderShouldBindIocContainerBindIocContainerTrue()
        {
            Type typeFrom = null;
            object item = null;
            string name = null;
            var iocContainer = new IocContainerMock
            {
                BindToConstantFunc = (type, arg2, arg3) =>
                {
                    typeFrom = type;
                    item = arg2;
                    name = arg3;
                }
            };
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            provider.BindIocContainer = true;
            var context = new DataContext();

            IViewModel viewModel = provider.GetViewModel(container => new ViewModel(), context);
            typeFrom.ShouldEqual(typeof(IIocContainer));
            item.ShouldEqual(viewModel.IocContainer);
            name.ShouldBeNull();
        }

        [TestMethod]
        public void ProviderShouldNotBindIocContainerBindIocContainerFalse()
        {
            bool isInvoked = false;
            var iocContainer = new IocContainerMock
            {
                BindToConstantFunc = (type, arg2, arg3) =>
                {
                    isInvoked = true;
                }
            };
            ViewModelProvider provider = GetViewModelProvider(iocContainer);
            provider.BindIocContainer = false;
            var context = new DataContext();

            IViewModel viewModel = provider.GetViewModel(container => new ViewModel(), context);
            isInvoked.ShouldBeFalse();
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
            viewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
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
            parentViewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeFalse();
            viewModel.ViewModelEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
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
            parentViewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.ViewModelEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeFalse();
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
            parentViewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.ViewModelEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
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
            parentViewModel.ViewModelEventAggregator.GetObservers().Contains(viewModel).ShouldBeTrue();
            viewModel.ViewModelEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
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
        public void ProviderShouldRestoreViewModelState()
        {
            DataConstant<string> key = "key";
            const string value = "value";
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
            var viewModel = new ViewModel();
            viewModel.Settings.State.Add(key, value);
            ViewModelProvider provider = GetViewModelProvider(iocContainer);

            var state = provider.PreserveViewModel(viewModel, DataContext.Empty);
            var restoreViewModel = provider.RestoreViewModel(state, DataContext.Empty, true);
            restoreViewModel.IocContainer.ShouldEqual(childIoc);
            restoreViewModel.ShouldEqual(loadViewModel);
            restoreViewModel.Settings.State.GetData(key).ShouldEqual(value);
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModel()
        {
            var childIoc = new IocContainerMock();
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = parentIoc.GetFunc;

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { CreateChild = mock => parentIoc });
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.ParentViewModel},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, DataContext.Empty, true);
            vm = (ViewModel)provider.RestoreViewModel(state, DataContext.Empty, true);

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(parentIoc);
            vm.IocContainer.ShouldEqual(childIoc);
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModelAfterChildViewModelRestore()
        {
            var childIoc = new IocContainerMock();
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = parentIoc.GetFunc;

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { CreateChild = mock => parentIoc });
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.ParentViewModel},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            vm = (ViewModel)provider.RestoreViewModel(state, DataContext.Empty, true);
            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, DataContext.Empty, true);

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(parentIoc);
            vm.IocContainer.ShouldNotEqual(childIoc);

            childIoc.GetFunc = (type, s, arg3) => childIoc;
            vm.IocContainer.Get(typeof(object)).ShouldEqual(childIoc);
        }

        [TestMethod]
        public void ProviderShouldRestoreIocContainerParentViewModelRestoreChildDuringRestoration()
        {
            var childIoc = new IocContainerMock();
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = parentIoc.GetFunc;

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { CreateChild = mock => parentIoc });
            var context = new DataContext
            {
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.ParentViewModel},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            ViewModel.InitializeViewModelStatic = dataContext =>
            {
                ViewModel.InitializeViewModelStatic = null;
                vm = (ViewModel)provider.RestoreViewModel(state, DataContext.Empty, true);
            };
            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, DataContext.Empty, true);

            vm.GetParentViewModel().ShouldEqual(parentViewModel);
            parentViewModel.IocContainer.ShouldEqual(parentIoc);
            vm.IocContainer.ShouldEqual(childIoc);
        }

        [TestMethod]
        public void ProviderShouldRestoreObservationMode()
        {
            var childIoc = new IocContainerMock();
            var parentIoc = new IocContainerMock
            {
                GetFunc = (type, s, arg3) =>
                {
                    type.ShouldEqual(typeof(ViewModel));
                    return new ViewModel();
                },
                CreateChild = mock => childIoc
            };
            childIoc.GetFunc = parentIoc.GetFunc;

            var parentViewModel = new ViewModel { IocContainer = new IocContainerMock() };
            var provider = GetViewModelProvider(new IocContainerMock { CreateChild = mock => parentIoc });
            var context = new DataContext
            {
                {InitializationConstants.ObservationMode, ObservationMode.Both},
                {InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Default},
                {InitializationConstants.ParentViewModel, parentViewModel}
            };

            var vm = new ViewModel();
            provider.InitializeViewModel(vm, context);

            var parentState = provider.PreserveViewModel(parentViewModel, DataContext.Empty);
            var state = provider.PreserveViewModel(vm, DataContext.Empty);

            parentViewModel = (ViewModel)provider.RestoreViewModel(parentState, DataContext.Empty, true);
            vm = (ViewModel)provider.RestoreViewModel(state, DataContext.Empty, true);

            vm.ViewModelEventAggregator.GetObservers().Contains(parentViewModel).ShouldBeTrue();
            parentViewModel.ViewModelEventAggregator.GetObservers().Contains(vm).ShouldBeTrue();
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
            var ctx = new DataContext { { InitializationConstants.IgnoreRestoredViewModelCache, true } };
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

        protected virtual ViewModelProvider GetViewModelProvider(IIocContainer iocContainer,
            bool bindIocContainer = false)
        {
            return new ViewModelProvider(iocContainer, bindIocContainer);
        }

        #endregion
    }
}