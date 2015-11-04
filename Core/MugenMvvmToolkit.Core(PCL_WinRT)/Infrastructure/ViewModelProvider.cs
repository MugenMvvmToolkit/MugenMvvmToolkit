#region Copyright

// ****************************************************************************
// <copyright file="ViewModelProvider.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Reflection;
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

        private sealed class RestoredViewModel
        {
            #region Fields

            private WeakReference _viewModelRef;
            private List<WeakReference> _childViewModels;

            #endregion

            #region Methods

            public IViewModel GetViewModel()
            {
                if (_viewModelRef == null)
                    return null;
                return (IViewModel)_viewModelRef.Target;
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
        protected static readonly DataConstant<Guid> IdParentViewModelConstant;

        private static readonly DataConstant<Guid> IdViewModelConstant;
        private static readonly Dictionary<Guid, RestoredViewModel> RestoredViewModels;

        private readonly IIocContainer _iocContainer;

        #endregion

        #region Constructors

        static ViewModelProvider()
        {
            RestoredViewModels = new Dictionary<Guid, RestoredViewModel>();
            ViewModelTypeNameConstant = DataConstant.Create(() => ViewModelTypeNameConstant, true);
            IdViewModelConstant = DataConstant.Create(() => IdViewModelConstant);
            IdParentViewModelConstant = DataConstant.Create(() => IdParentViewModelConstant);
            Tracer.TraceViewModelHandler += OnTraceViewModel;
        }

        public ViewModelProvider([NotNull]IIocContainer iocContainer)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            _iocContainer = iocContainer;
        }

        #endregion

        #region Properties

        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        #endregion

        #region Implementation of IViewModelProvider

        public IViewModel TryGetViewModelById(Guid viewModelId)
        {
            return TryGetViewModelByIdInternal(viewModelId);
        }

        public IViewModel GetViewModel(GetViewModelDelegate<IViewModel> getViewModel, IDataContext dataContext)
        {
            Should.NotBeNull(getViewModel, "getViewModel");
            Should.NotBeNull(dataContext, "dataContext");
            dataContext = dataContext.ToNonReadOnly();
            IViewModel viewModel = getViewModel(GetIocContainer(dataContext));
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext);
            return viewModel;
        }

        public IViewModel GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            Should.NotBeNull(viewModelType, "viewModelType");
            Should.NotBeNull(dataContext, "dataContext");
            var viewModel = CreateViewModel(viewModelType, dataContext);
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext);
            return viewModel;
        }

        public void InitializeViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
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
            Should.NotBeNull(viewModel, "viewModel");
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
                state.AddOrUpdate(IdParentViewModelConstant, idParent);
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
                    if (viewModelState.TryGetData(IdViewModelConstant, out id))
                    {
                        viewModel = GetOrAddRestoredViewModel(id).GetViewModel();
                        if (viewModel != null)
                            return viewModel;
                    }
                }

                RestoredViewModel restoredParentViewModel = null;
                IViewModel parentViewModel = null;
                Guid idParent;
                if (viewModelState.TryGetData(IdParentViewModelConstant, out idParent))
                {
                    restoredParentViewModel = GetOrAddRestoredViewModel(idParent);
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
                    Tracer.TraceViewModel(AuditAction.Restored, viewModel);
                    if (ReferenceEquals(viewModelState, DataContext.Empty))
                        Tracer.Warn("The view model '{0}' was restored without state.", viewModel);
                    return viewModel;
                }

                if (throwOnError)
                    throw ExceptionManager.ViewModelCannotBeRestored();
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw;
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
            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.SaveState(state);
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

            dataContext.AddOrUpdate(ViewModelConstants.StateRestored, true);
            dataContext.AddOrUpdate(InitializationConstants.IsRestored, true);

            var viewModel = CreateViewModel(vmType, dataContext);
            IDataContext vmState = viewModel.Settings.State;
            vmState.Merge(viewModelState);

            InitializeViewModel(viewModel, dataContext);

            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.LoadState(vmState);
            return viewModel;
        }

        protected virtual void OnViewModelInitializing([NotNull]IViewModel viewModel, [NotNull] IDataContext dataContext)
        {
            var id = GetOrAddViewModelId(viewModel);
            GetOrAddRestoredViewModel(id).SetViewModel(viewModel);
        }

        protected virtual void InitializeViewModelInternal([NotNull] IViewModel viewModel, [NotNull] IDataContext dataContext)
        {
            dataContext = dataContext.ToNonReadOnly();
            var parentViewModel = dataContext.GetData(InitializationConstants.ParentViewModel);
            OnViewModelInitializing(viewModel, dataContext);
            InitializeParentViewModel(viewModel, parentViewModel, dataContext);
            viewModel.InitializeViewModel(dataContext);
            InitializeDisplayName(viewModel);
            MergeParameters(dataContext, viewModel.Settings.Metadata);
            if (parentViewModel != null)
            {
                var parentAwareViewModel = viewModel as IParentAwareViewModel;
                if (parentAwareViewModel != null)
                    parentAwareViewModel.SetParent(parentViewModel);
            }
        }

        protected virtual IViewModel TryGetViewModelByIdInternal(Guid viewModelId)
        {
            lock (RestoredViewModels)
            {
                RestoredViewModel value;
                if (RestoredViewModels.TryGetValue(viewModelId, out value))
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

        private void InitializeDisplayName(IViewModel viewModel)
        {
            IIocContainer iocContainer = viewModel.GetIocContainer(true, false) ?? IocContainer;
            var hasDisplayName = viewModel as IHasDisplayName;
            IDisplayNameProvider displayNameProvider;
            if (hasDisplayName != null && string.IsNullOrEmpty(hasDisplayName.DisplayName)
                && iocContainer.TryGet(out displayNameProvider))
                hasDisplayName.DisplayName = displayNameProvider
#if PCL_WINRT
.GetDisplayNameAccessor(viewModel.GetType().GetTypeInfo())
#else
.GetDisplayNameAccessor(viewModel.GetType())
#endif
.Invoke();
        }

        internal static Guid GetOrAddViewModelId(IViewModel viewModel)
        {
            lock (IdViewModelConstant)
            {
                Guid id;
                if (!viewModel.Settings.State.TryGetData(IdViewModelConstant, out id))
                {
                    id = Guid.NewGuid();
                    viewModel.Settings.State.Add(IdViewModelConstant, id);
                }
                return id;
            }
        }

        private static RestoredViewModel GetOrAddRestoredViewModel(Guid id)
        {
            lock (RestoredViewModels)
            {
                RestoredViewModel value;
                if (!RestoredViewModels.TryGetValue(id, out value))
                {
                    value = new RestoredViewModel();
                    RestoredViewModels[id] = value;
                }
                return value;
            }
        }

        private static void OnTraceViewModel(AuditAction auditAction, IViewModel viewModel)
        {
            if (auditAction != AuditAction.Disposed && auditAction != AuditAction.Finalized)
                return;

            Guid id;
            if (!viewModel.Settings.State.TryGetData(IdViewModelConstant, out id))
                return;

            RestoredViewModel value;
            lock (RestoredViewModels)
            {
                if (RestoredViewModels.TryGetValue(id, out value))
                    RestoredViewModels.Remove(id);
            }
            if (value != null && auditAction == AuditAction.Disposed)
                value.Clear();
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
            var parentAwareViewModel = viewModel as IParentAwareViewModel;
            if (parentAwareViewModel != null)
                parentAwareViewModel.SetParent(parentViewModel);
        }

        #endregion
    }
}
