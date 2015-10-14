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
using MugenMvvmToolkit.Models.IoC;
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

            public void SetViewModel(IViewModel viewModel, IIocContainer container)
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
                    _childViewModels.Clear();
                }
                for (int i = 0; i < viewModels.Length; i++)
                {
                    var childVm = (IViewModel)viewModels[i].Target;
                    if (childVm != null)
                        OnParentUpdated(childVm, viewModel, container);
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
                    OnParentUpdated(viewModel, target, viewModel.IocContainer);
            }

            public void Clear()
            {
                _childViewModels = null;
                _viewModelRef = Empty.WeakReference;
            }

            #endregion
        }

        private sealed class ParentIocContainerWrapper : IIocContainer
        {
            #region Fields

            private readonly object _locker;
            private readonly ParentIocContainerWrapper _parent;
            private List<ParentIocContainerWrapper> _children;
            private readonly bool _isMixed;
            private bool _parentInitialized;

            private IIocContainer _iocContainer;
            private IIocContainer _parentContainer;

            #endregion

            #region Constructors

            private ParentIocContainerWrapper(bool isMixed, ParentIocContainerWrapper parent)
            {
                _locker = new object();
                _isMixed = isMixed;
                _parent = parent;
            }

            public ParentIocContainerWrapper(IIocContainer container, ParentIocContainerWrapper parent = null)
                : this(false, parent)
            {
                _parentContainer = container.CreateChild();
                _iocContainer = container;
                _iocContainer.Disposed += ContainerOnDisposed;
            }

            public ParentIocContainerWrapper(IIocContainer container, IIocContainer parentContainer, ParentIocContainerWrapper parent = null)
                : this(true, parent)
            {
                if (!IsContainerDisposed(parentContainer))
                {
                    _parentContainer = parentContainer.CreateChild();
                    _parentInitialized = true;
                }
                _iocContainer = container.CreateChild();
                if (_parentContainer == null)
                    _parentContainer = _iocContainer;
                _iocContainer.Disposed += ContainerOnDisposed;
            }

            #endregion

            #region Methods

            public void SetParentContainer(IIocContainer parentContainer)
            {
                if (_parentInitialized || _iocContainer.IsDisposed || IsContainerDisposed(parentContainer))
                    return;
                lock (_locker)
                {
                    if (_parentInitialized)
                        return;
                    _parentInitialized = true;
                    _parentContainer = parentContainer.CreateChild();
                    if (!_isMixed)
                    {
                        _iocContainer.Disposed -= ContainerOnDisposed;
                        _iocContainer.Dispose();
                        _iocContainer = _parentContainer;
                        _iocContainer.Disposed += ContainerOnDisposed;
                    }
                }
                if (_children == null)
                    return;
                for (int i = 0; i < _children.Count; i++)
                    _children[i].SetParentContainer(_parentContainer);
                _children = null;
            }

            private IIocContainer IocContainer
            {
                get
                {
                    if (IsContainerDisposed(_parentContainer))
                        return _iocContainer;
                    return _parentContainer;
                }
            }

            private static bool IsContainerDisposed(IIocContainer container)
            {
                if (container == null)
                    return true;
                while (container != null)
                {
                    if (container.IsDisposed)
                        return true;
                    container = container.Parent;
                }
                return false;
            }

            private void ContainerOnDisposed(IDisposableObject sender, EventArgs args)
            {
                sender.Disposed -= ContainerOnDisposed;
                Dispose();
            }

            #endregion

            #region Implementation of IIocContainer

            public void Dispose()
            {
                _iocContainer.Disposed -= ContainerOnDisposed;
                _parentContainer.Dispose();
                _iocContainer.Dispose();
                var disposed = Disposed;
                if (disposed != null)
                    disposed(this, EventArgs.Empty);
                Disposed = null;
            }

            public bool IsDisposed
            {
                get { return _iocContainer.IsDisposed; }
            }

            public event EventHandler<IDisposableObject, EventArgs> Disposed;

            public object GetService(Type serviceType)
            {
                return IocContainer.GetService(serviceType);
            }

            public int Id
            {
                get { return _iocContainer.Id; }
            }

            public IIocContainer Parent
            {
                get { return _parent ?? IocContainer.Parent; }
            }

            public object Container
            {
                get { return IocContainer; }
            }

            public IIocContainer CreateChild()
            {
                lock (_locker)
                {
                    if (_parentInitialized)
                    {
                        if (_isMixed)
                            return new ParentIocContainerWrapper(_iocContainer, _parentContainer, this);
                        return _parentContainer.CreateChild();
                    }

                    if (_children == null)
                        _children = new List<ParentIocContainerWrapper>();
                    var child = _isMixed
                        ? new ParentIocContainerWrapper(_iocContainer, null, this)
                        : new ParentIocContainerWrapper(_iocContainer, this);
                    _children.Add(child);
                    return child;
                }
            }

            public object Get(Type service, string name = null, params IIocParameter[] parameters)
            {
                return IocContainer.Get(service, name, parameters);
            }

            public IEnumerable<object> GetAll(Type service, string name = null, params IIocParameter[] parameters)
            {
                return IocContainer.GetAll(service, name, parameters);
            }

            public void BindToConstant(Type service, object instance, string name = null)
            {
                if (_isMixed && _parentInitialized)
                {
                    if (!IsContainerDisposed(_parentContainer))
                        _parentContainer.BindToConstant(service, instance, name);
                    _iocContainer.BindToConstant(service, instance, name);
                }
                else
                    IocContainer.BindToConstant(service, instance, name);
            }

            public void BindToMethod(Type service,
                Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle,
                string name = null, params IIocParameter[] parameters)
            {
                if (_isMixed && _parentInitialized)
                {
                    if (!IsContainerDisposed(_parentContainer))
                        _parentContainer.BindToMethod(service, methodBindingDelegate, lifecycle, name, parameters);
                    _iocContainer.BindToMethod(service, methodBindingDelegate, lifecycle, name, parameters);
                }
                else
                    IocContainer.BindToMethod(service, methodBindingDelegate, lifecycle, name, parameters);
            }

            public void Bind(Type service, Type typeTo, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters)
            {
                if (_isMixed && _parentInitialized)
                {
                    if (!IsContainerDisposed(_parentContainer))
                        _parentContainer.Bind(service, typeTo, lifecycle, name, parameters);
                    _iocContainer.Bind(service, typeTo, lifecycle, name, parameters);
                }
                else
                    IocContainer.Bind(service, typeTo, lifecycle, name, parameters);
            }

            public void Unbind(Type service)
            {
                if (_isMixed && _parentInitialized)
                {
                    if (!IsContainerDisposed(_parentContainer))
                        _parentContainer.Unbind(service);
                    _iocContainer.Unbind(service);
                }
                else
                    IocContainer.Unbind(service);
            }

            public bool CanResolve(Type service, string name = null)
            {
                return IocContainer.CanResolve(service, name);
            }

            #endregion
        }

        #endregion

        #region Fields

        protected static readonly DataConstant<string> ViewModelTypeNameConstant;
        protected static readonly DataConstant<Guid> IdViewModelConstant;
        protected static readonly DataConstant<Guid> IdParentViewModelConstant;
        private static readonly object IdGeneratorLocker;
        private static readonly Dictionary<Guid, RestoredViewModel> RestoredViewModels;

        private readonly IIocContainer _iocContainer;

        #endregion

        #region Constructors

        static ViewModelProvider()
        {
            IdGeneratorLocker = new object();
            RestoredViewModels = new Dictionary<Guid, RestoredViewModel>();
            ViewModelTypeNameConstant = DataConstant.Create(() => ViewModelTypeNameConstant, true);
            IdViewModelConstant = DataConstant.Create(() => IdViewModelConstant);
            IdParentViewModelConstant = DataConstant.Create(() => IdParentViewModelConstant);
            Tracer.TraceViewModelHandler += OnTraceViewModel;
        }

        public ViewModelProvider([NotNull] IIocContainer iocContainer, bool bindIocContainer = false)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            _iocContainer = iocContainer;
            BindIocContainer = bindIocContainer;
        }

        #endregion

        #region Properties

        public bool BindIocContainer { get; set; }

        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        #endregion

        #region Implementation of IViewModelProvider

        public IViewModel GetViewModel(GetViewModelDelegate<IViewModel> getViewModel, IDataContext dataContext)
        {
            Should.NotBeNull(getViewModel, "getViewModel");
            Should.NotBeNull(dataContext, "dataContext");
            dataContext = dataContext.ToNonReadOnly();
            IIocContainer iocContainer = CreateViewModelIocContainer(dataContext);
            IViewModel viewModel = getViewModel(iocContainer);
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext, iocContainer);
            return viewModel;
        }

        public IViewModel GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            Should.NotBeNull(viewModelType, "viewModelType");
            Should.NotBeNull(dataContext, "dataContext");
            string viewModelBindingName = dataContext.GetData(InitializationConstants.ViewModelBindingName);
            IIocParameter[] parameters = dataContext.GetData(InitializationConstants.IocParameters);
            IIocContainer iocContainer = CreateViewModelIocContainer(dataContext);
            var viewModel = (IViewModel)iocContainer.Get(viewModelType, viewModelBindingName, parameters);
            if (!viewModel.IsInitialized)
                InitializeViewModel(viewModel, dataContext, iocContainer);
            return viewModel;
        }

        public void InitializeViewModel(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            InitializeViewModel(viewModel, dataContext ?? DataContext.Empty, null);
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
                if (!dataContext.GetData(InitializationConstants.IgnoreRestoredViewModelCache))
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

                viewModel = RestoreViewModel(viewModelState, dataContext);
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

        public event EventHandler<IViewModelProvider, ViewModelPreservingEventArgs> Preserving;

        public event EventHandler<IViewModelProvider, ViewModelPreservedEventArgs> Preserved;

        public event EventHandler<IViewModelProvider, ViewModelRestoringEventArgs> Restoring;

        public event EventHandler<IViewModelProvider, ViewModelRestoredEventArgs> Restored;

        #endregion

        #region Methods

        public static IViewModel TryGetViewModelById(Guid idViewModel)
        {
            lock (RestoredViewModels)
            {
                RestoredViewModel value;
                if (RestoredViewModels.TryGetValue(idViewModel, out value))
                    return value.GetViewModel();
                return null;
            }
        }

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
        protected virtual IViewModel RestoreViewModel([NotNull] IDataContext viewModelState, [NotNull] IDataContext dataContext)
        {
            string typeName = viewModelState.GetData(ViewModelTypeNameConstant);
            Type vmType = typeName == null
                ? dataContext.GetData(InitializationConstants.ViewModelType)
                : Type.GetType(typeName, false);
            if (vmType == null)
                return null;

            dataContext.AddOrUpdate(ViewModelConstants.StateRestored, true);
            dataContext.AddOrUpdate(InitializationConstants.IsRestored, true);

            IIocContainer container = CreateViewModelIocContainer(dataContext);
            var viewModel = (IViewModel)container.Get(vmType);
            IDataContext vmState = viewModel.Settings.State;
            vmState.Merge(viewModelState);

            InitializeViewModel(viewModel, dataContext, container);

            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.LoadState(vmState);
            return viewModel;
        }

        protected virtual void OnViewModelPreserved([NotNull] IViewModel viewModel, [NotNull] IDataContext viewModelState,
            [NotNull] IDataContext dataContext)
        {
        }

        protected virtual void OnViewModelRestored([NotNull] IViewModel viewModel, [NotNull] IDataContext viewModelState,
            [NotNull] IDataContext dataContext)
        {
        }

        protected virtual void OnViewModelInitializing([NotNull]IViewModel viewModel, [NotNull] IDataContext dataContext)
        {
            if (!dataContext.GetData(InitializationConstants.IsRestored))
                return;
            Guid id;
            if (viewModel.Settings.State.TryGetData(IdViewModelConstant, out id))
                GetOrAddRestoredViewModel(id).SetViewModel(viewModel, dataContext.GetData(InitializationConstants.IocContainer));
            var viewModelBase = viewModel as ViewModelBase;
            if (viewModelBase != null)
                viewModelBase.IocContainer = dataContext.GetData(InitializationConstants.IocContainer);
        }

        protected virtual void InitializeViewModel([NotNull] IViewModel viewModel, [NotNull] IDataContext dataContext,
            [CanBeNull] IIocContainer iocContainer)
        {
            dataContext = dataContext.ToNonReadOnly();
            if (iocContainer == null)
                iocContainer = CreateViewModelIocContainer(dataContext);
            dataContext.AddOrUpdate(InitializationConstants.IocContainer, iocContainer);
            OnViewModelInitializing(viewModel, dataContext);
            InitializeParentViewModel(viewModel, dataContext.GetData(InitializationConstants.ParentViewModel), dataContext);
            viewModel.InitializeViewModel(dataContext);
            InitializeDisplayName(viewModel);
            MergeParameters(dataContext, viewModel.Settings.Metadata);
        }

        protected virtual IIocContainer CreateViewModelIocContainer([NotNull] IDataContext dataContext)
        {
            IIocContainer container = dataContext.GetData(InitializationConstants.IocContainer);
            if (container == null)
            {
                IocContainerCreationMode creationMode;
                if (!dataContext.TryGetData(InitializationConstants.IocContainerCreationMode, out creationMode))
                    creationMode = ApplicationSettings.IocContainerCreationMode;
                IViewModel parentViewModel = dataContext.GetData(InitializationConstants.ParentViewModel);
                var isRestored = dataContext.GetData(InitializationConstants.IsRestored);
                switch (creationMode)
                {
                    case IocContainerCreationMode.ParentViewModel:
                        if (parentViewModel != null)
                            container = parentViewModel.IocContainer.CreateChild();
                        else if (isRestored && dataContext.Contains(IdParentViewModelConstant))
                            container = new ParentIocContainerWrapper(_iocContainer);
                        break;
                    case IocContainerCreationMode.Mixed:
                        var parentContainer = parentViewModel == null ? null : parentViewModel.IocContainer;
                        if (parentContainer != null || (isRestored && dataContext.Contains(IdParentViewModelConstant)))
                            container = new ParentIocContainerWrapper(_iocContainer, parentContainer);
                        break;
                }

                if (container == null)
                    container = _iocContainer.CreateChild();
            }
            if (BindIocContainer)
                container.BindToConstant(container);
            return container;
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

        internal static Guid GetOrAddViewModelId(IViewModel viewModel)
        {
            lock (IdGeneratorLocker)
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

            IocContainerCreationMode creationMode;
            if (ctxFrom.TryGetData(InitializationConstants.IocContainerCreationMode, out creationMode))
                ctxTo.AddOrUpdate(InitializationConstants.IocContainerCreationMode, creationMode);

            ObservationMode observationMode;
            if (ctxFrom.TryGetData(InitializationConstants.ObservationMode, out observationMode))
                ctxTo.AddOrUpdate(InitializationConstants.ObservationMode, observationMode);
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

        private static void OnParentUpdated(IViewModel viewModel, IViewModel parentViewModel, IIocContainer container)
        {
            InitializeParentViewModel(viewModel, parentViewModel, viewModel.Settings.Metadata);
            var containerWrapper = viewModel.IocContainer as ParentIocContainerWrapper;
            if (containerWrapper != null)
                containerWrapper.SetParentContainer(container);
        }

        #endregion
    }
}
