#region Copyright

// ****************************************************************************
// <copyright file="ViewModelProvider.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ViewModelProvider : IViewModelProvider
    {
        #region Nested types

        private sealed class CachedViewModel
        {
            #region Fields

            private WeakReference _viewModelRef;
            private List<WeakReference> _childViewModels;

            #endregion

            #region Methods

            public IViewModel GetViewModel()
            {
                return (IViewModel)_viewModelRef?.Target;
            }

            public void SetViewModel(IViewModel viewModel)
            {
                WeakReference[] viewModels;
                lock (this)
                {
                    if (_viewModelRef != null)
                        return;
                    _viewModelRef = ToolkitExtensions.GetWeakReference(viewModel);
                    if (_childViewModels == null)
                        return;
                    viewModels = _childViewModels.ToArray();
                    _childViewModels = null;
                }
                for (int i = 0; i < viewModels.Length; i++)
                {
                    var childVm = (IViewModel)viewModels[i].Target;
                    if (childVm != null && !childVm.IsDisposed)
                        OnParentUpdated(childVm, viewModel);
                }
            }

            public void AddChildViewModel(IViewModel viewModel)
            {
                lock (this)
                {
                    if (_viewModelRef == null)
                    {
                        if (_childViewModels == null)
                            _childViewModels = new List<WeakReference>();
                        _childViewModels.Add(ToolkitExtensions.GetWeakReference(viewModel));
                        return;
                    }
                }
                var target = (IViewModel)_viewModelRef.Target;
                if (target != null)
                    OnParentUpdated(viewModel, target);
            }

            public void Clear()
            {
                _childViewModels = null;
                _viewModelRef = Empty.WeakReference;
            }

            #endregion
        }

        #endregion

        #region Fields

        protected static readonly DataConstant<string> ViewModelTypeNameConstant;
        private static readonly Dictionary<Guid, CachedViewModel> CachedViewModels;

        private readonly IIocContainer _iocContainer;

        #endregion

        #region Constructors

        static ViewModelProvider()
        {
            CachedViewModels = new Dictionary<Guid, CachedViewModel>();
            ViewModelTypeNameConstant = DataConstant.Create<string>(typeof(ViewModelProvider), nameof(ViewModelTypeNameConstant), true);
            Tracer.TraceViewModelHandler += OnTraceViewModel;
        }

        public ViewModelProvider([NotNull]IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, nameof(iocContainer));
            _iocContainer = iocContainer;
        }

        #endregion

        #region Properties

        protected IIocContainer IocContainer => _iocContainer;

        #endregion

        #region Implementation of IViewModelProvider

        public IViewModel TryGetViewModelById(Guid viewModelId)
        {
            return TryGetViewModelByIdInternal(viewModelId);
        }

        public IList<IViewModel> GetCreatedViewModels(IDataContext dataContext = null)
        {
            return GetCreatedViewModelsInternal(dataContext ?? DataContext.Empty);
        }

        public IViewModel GetViewModel(GetViewModelDelegate<IViewModel> getViewModel, IDataContext dataContext)
        {
            Should.NotBeNull(getViewModel, nameof(getViewModel));
            Should.NotBeNull(dataContext, nameof(dataContext));
            dataContext = dataContext.ToNonReadOnly();
            IViewModel viewModel = getViewModel(GetIocContainer(dataContext));
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext);
            return viewModel;
        }

        public IViewModel GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = CreateViewModel(viewModelType, dataContext);
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext);
            return viewModel;
        }

        public void InitializeViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            ViewModelInitializationEventArgs args = null;
            var initializing = Initializing;
            if (initializing != null)
            {
                args = new ViewModelInitializationEventArgs(viewModel, dataContext);
                initializing(this, args);
                dataContext = args.Context;
            }
            InitializeViewModelInternal(viewModel, dataContext ?? DataContext.Empty);
            var initialized = Initialized;
            if (initialized != null)
            {
                if (args == null)
                    args = new ViewModelInitializationEventArgs(viewModel, dataContext);
                initialized(this, args);
            }
        }

        public IDataContext PreserveViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (dataContext == null)
                dataContext = DataContext.Empty;

            var preserving = Preserving;
            if (preserving != null)
            {
                var args = new ViewModelPreservingEventArgs(viewModel) { Context = dataContext };
                preserving(this, args);
                dataContext = args.Context ?? DataContext.Empty;
            }
            IDataContext state = PreserveViewModelInternal(viewModel, dataContext);

            GetOrAddViewModelId(viewModel);
            MergeParameters(viewModel.Settings.Metadata, viewModel.Settings.State);
            //Saving parent view model id.
            var parentViewModel = viewModel.GetParentViewModel();
            if (parentViewModel != null)
            {
                var idParent = GetOrAddViewModelId(parentViewModel);
                state.AddOrUpdate(ViewModelConstants.IdParent, idParent);
            }

            OnViewModelPreserved(viewModel, state, dataContext);

            var preserved = Preserved;
            if (preserved != null)
            {
                var args = new ViewModelPreservedEventArgs(viewModel) { Context = dataContext, State = state };
                preserved(this, args);
                return args.State;
            }
            return state;
        }

        public IViewModel RestoreViewModel(IDataContext viewModelState, IDataContext dataContext, bool throwOnError)
        {
            try
            {
                dataContext = dataContext.ToNonReadOnly();
                if (viewModelState == null)
                    viewModelState = DataContext.Empty;
                else
                    dataContext.Merge(viewModelState);

                IViewModel viewModel;
                if (!dataContext.GetData(InitializationConstants.IgnoreViewModelCache))
                {
                    Guid id;
                    if (viewModelState.TryGetData(ViewModelConstants.Id, out id))
                    {
                        viewModel = GetOrAddCachedViewModel(id).GetViewModel();
                        if (viewModel != null)
                            return viewModel;
                    }
                }

                CachedViewModel restoredParentViewModel = null;
                IViewModel parentViewModel = null;
                Guid idParent;
                if (viewModelState.TryGetData(ViewModelConstants.IdParent, out idParent))
                {
                    restoredParentViewModel = GetOrAddCachedViewModel(idParent);
                    parentViewModel = restoredParentViewModel.GetViewModel();
                    if (parentViewModel != null)
                        dataContext.AddOrUpdate(InitializationConstants.ParentViewModel, parentViewModel);
                }

                var restoring = Restoring;
                if (restoring != null)
                {
                    var args = new ViewModelRestoringEventArgs { Context = dataContext, ViewModelState = viewModelState };
                    restoring(this, args);
                    dataContext = args.Context ?? DataContext.Empty;
                }

                viewModel = RestoreViewModelInternal(viewModelState, dataContext);
                if (viewModel != null)
                {
                    if (restoredParentViewModel != null && parentViewModel == null)
                        restoredParentViewModel.AddChildViewModel(viewModel);
                    OnViewModelRestored(viewModel, viewModelState, dataContext);

                    var restored = Restored;
                    if (restored != null)
                    {
                        var args = new ViewModelRestoredEventArgs(viewModel)
                        {
                            Context = dataContext,
                            ViewModelState = viewModelState
                        };
                        restored(this, args);
                    }
                    Tracer.TraceViewModel(ViewModelLifecycleType.Restored, viewModel);
                    if (ReferenceEquals(viewModelState, DataContext.Empty))
                        Tracer.Warn("The view model '{0}' was restored without state.", viewModel);
                    return viewModel;
                }

                if (throwOnError)
                    throw ExceptionManager.ViewModelCannotBeRestored();
            }
            catch (Exception e) when (!throwOnError)
            {
                Tracer.Warn(e.Flatten(true));
            }
            return null;
        }

        public event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initializing;

        public event EventHandler<IViewModelProvider, ViewModelInitializationEventArgs> Initialized;

        public event EventHandler<IViewModelProvider, ViewModelPreservingEventArgs> Preserving;

        public event EventHandler<IViewModelProvider, ViewModelPreservedEventArgs> Preserved;

        public event EventHandler<IViewModelProvider, ViewModelRestoringEventArgs> Restoring;

        public event EventHandler<IViewModelProvider, ViewModelRestoredEventArgs> Restored;

        #endregion

        #region Methods

        [NotNull]
        protected virtual IDataContext PreserveViewModelInternal(IViewModel viewModel, IDataContext dataContext)
        {
            IDataContext state = viewModel.Settings.State;
            state.AddOrUpdate(ViewModelTypeNameConstant, viewModel.GetType().AssemblyQualifiedName);
            (viewModel as IHasState)?.SaveState(state);
            return state;
        }

        [CanBeNull]
        protected virtual IViewModel RestoreViewModelInternal([NotNull] IDataContext viewModelState, [NotNull] IDataContext dataContext)
        {
            string typeName = viewModelState.GetData(ViewModelTypeNameConstant);
            Type vmType = typeName == null
                ? dataContext.GetData(InitializationConstants.ViewModelType)
                : Type.GetType(typeName, false);
            if (vmType == null)
                return null;

            if (!dataContext.Contains(InitializationConstants.IocParameters))
            {
                var parameters = viewModelState.GetData(InitializationConstants.IocParameters);
                if (parameters != null)
                    dataContext.AddOrUpdate(InitializationConstants.IocParameters, parameters);
            }
            dataContext.AddOrUpdate(ViewModelConstants.StateRestored, true);
            dataContext.AddOrUpdate(InitializationConstants.IsRestored, true);

            var viewModel = CreateViewModel(vmType, dataContext);
            IDataContext vmState = viewModel.Settings.State;
            vmState.Merge(viewModelState);

            InitializeViewModel(viewModel, dataContext);
            (viewModel as IHasState)?.LoadState(vmState);
            return viewModel;
        }

        protected virtual void OnViewModelInitializing([NotNull]IViewModel viewModel, [NotNull] IDataContext dataContext)
        {
            var id = GetOrAddViewModelId(viewModel);
            GetOrAddCachedViewModel(id).SetViewModel(viewModel);
        }

        protected virtual void InitializeViewModelInternal([NotNull] IViewModel viewModel, [NotNull] IDataContext dataContext)
        {
            dataContext = dataContext.ToNonReadOnly();
            var parentViewModel = dataContext.GetData(InitializationConstants.ParentViewModel);
            OnViewModelInitializing(viewModel, dataContext);
            InitializeParentViewModel(viewModel, parentViewModel, dataContext);
            viewModel.InitializeViewModel(dataContext);
            MergeParameters(dataContext, viewModel.Settings.Metadata);
            if (parentViewModel != null)
                (viewModel as IParentAwareViewModel)?.SetParent(parentViewModel);
        }

        protected virtual IList<IViewModel> GetCreatedViewModelsInternal([NotNull]IDataContext context)
        {
            var result = new List<IViewModel>();
            lock (CachedViewModels)
            {
                foreach (var value in CachedViewModels.Values)
                {
                    var viewModel = value.GetViewModel();
                    if (viewModel != null)
                        result.Add(viewModel);
                }
            }
            return result;
        }

        protected virtual IViewModel TryGetViewModelByIdInternal(Guid viewModelId)
        {
            lock (CachedViewModels)
            {
                CachedViewModel value;
                if (CachedViewModels.TryGetValue(viewModelId, out value))
                    return value.GetViewModel();
                return null;
            }
        }

        protected virtual void OnViewModelPreserved([NotNull] IViewModel viewModel, [NotNull] IDataContext viewModelState, [NotNull] IDataContext dataContext)
        {
        }

        protected virtual void OnViewModelRestored([NotNull] IViewModel viewModel, [NotNull] IDataContext viewModelState, [NotNull] IDataContext dataContext)
        {
        }

        protected virtual IViewModel CreateViewModel([NotNull]Type viewModelType, [NotNull] IDataContext context)
        {
            string viewModelBindingName = context.GetData(InitializationConstants.ViewModelBindingName);
            IIocParameter[] parameters = context.GetData(InitializationConstants.IocParameters);
            return (IViewModel)GetIocContainer(context).Get(viewModelType, viewModelBindingName, parameters);
        }

        protected virtual IIocContainer GetIocContainer([NotNull]IDataContext context)
        {
            var iocContainer = context.GetData(InitializationConstants.IocContainer);
            if (iocContainer != null)
                return iocContainer;
            IViewModel parent = null;
            var parentViewModel = context.GetData(ViewModelConstants.ParentViewModel);
            if (parentViewModel != null)
                parent = parentViewModel.Target as IViewModel;
            if (parent == null)
                return IocContainer;
            return parent.GetIocContainer(false) ?? IocContainer;
        }

        internal static Guid GetOrAddViewModelId(IViewModel viewModel)
        {
            lock (ViewModelConstants.Id)
            {
                Guid id;
                if (!viewModel.Settings.State.TryGetData(ViewModelConstants.Id, out id))
                {
                    id = Guid.NewGuid();
                    viewModel.Settings.State.Add(ViewModelConstants.Id, id);
                }
                return id;
            }
        }

        private static CachedViewModel GetOrAddCachedViewModel(Guid id)
        {
            lock (CachedViewModels)
            {
                CachedViewModel value;
                if (!CachedViewModels.TryGetValue(id, out value))
                {
                    value = new CachedViewModel();
                    CachedViewModels[id] = value;
                }
                return value;
            }
        }

        private static void OnTraceViewModel(ViewModelLifecycleType lifecycleType, IViewModel viewModel)
        {
            if (lifecycleType != ViewModelLifecycleType.Disposed && lifecycleType != ViewModelLifecycleType.Finalized)
                return;

            Guid id;
            if (!viewModel.Settings.State.TryGetData(ViewModelConstants.Id, out id))
                return;
            if (lifecycleType == ViewModelLifecycleType.Finalized)
            {
                Task.Factory.StartNew(state =>
                {
                    lock (CachedViewModels)
                        CachedViewModels.Remove((Guid)state);
                }, id);
                return;
            }

            CachedViewModel value;
            lock (CachedViewModels)
            {
                if (CachedViewModels.TryGetValue(id, out value))
                    CachedViewModels.Remove(id);
            }
            value?.Clear();
        }

        private static void MergeParameters(IDataContext ctxFrom, IDataContext ctxTo)
        {
            string viewName = ctxFrom.GetData(InitializationConstants.ViewName);
            if (!string.IsNullOrEmpty(viewName))
                ctxTo.AddOrUpdate(InitializationConstants.ViewName, viewName);

            ObservationMode observationMode;
            if (ctxFrom.TryGetData(InitializationConstants.ObservationMode, out observationMode))
                ctxTo.AddOrUpdate(InitializationConstants.ObservationMode, observationMode);

            string bindingName = ctxFrom.GetData(InitializationConstants.ViewModelBindingName);
            if (!string.IsNullOrEmpty(bindingName))
                ctxTo.AddOrUpdate(InitializationConstants.ViewModelBindingName, bindingName);
        }

        private static void InitializeParentViewModel(IViewModel viewModel, IViewModel parentViewModel, IDataContext context)
        {
            if (parentViewModel == null)
                return;
            ObservationMode observationMode;
            if (!context.TryGetData(InitializationConstants.ObservationMode, out observationMode))
                observationMode = ApplicationSettings.ViewModelObservationMode;
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.ParentViewModel, ToolkitExtensions.GetWeakReference(parentViewModel));
            if (observationMode.HasFlagEx(ObservationMode.ParentObserveChild))
                viewModel.Subscribe(parentViewModel);
            if (observationMode.HasFlagEx(ObservationMode.ChildObserveParent))
                parentViewModel.Subscribe(viewModel);
        }

        private static void OnParentUpdated(IViewModel viewModel, IViewModel parentViewModel)
        {
            InitializeParentViewModel(viewModel, parentViewModel, viewModel.Settings.Metadata);
            (viewModel as IParentAwareViewModel)?.SetParent(parentViewModel);
        }

        #endregion
    }
}
