using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Views
{
    public class ViewManager : HasListenersBase<IViewManagerListener>, IViewManager
    {
        #region Fields

        private static readonly Dictionary<Type, PropertyInfo> ViewToViewModelInterface;
        private static readonly Dictionary<Type, PropertyInfo> ViewModelToViewInterface;

        #endregion

        #region Constructors

        static ViewManager()
        {
            ViewToViewModelInterface = new Dictionary<Type, PropertyInfo>(MemberInfoComparer.Instance);
            ViewModelToViewInterface = new Dictionary<Type, PropertyInfo>(MemberInfoComparer.Instance);
        }

        [Preserve(Conditional = true)]
        public ViewManager(IServiceProvider serviceProvider, IThreadDispatcher threadDispatcher, IWrapperManager wrapperManager, ITracer tracer, IViewDataContextProvider dataContextProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(dataContextProvider, nameof(dataContextProvider));
            ServiceProvider = serviceProvider;
            ThreadDispatcher = threadDispatcher;
            WrapperManager = wrapperManager;
            Tracer = tracer;
            DataContextProvider = dataContextProvider;
        }

        #endregion

        #region Properties

        protected IServiceProvider ServiceProvider { get; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected IWrapperManager WrapperManager { get; }

        protected ITracer Tracer { get; }

        protected IViewDataContextProvider DataContextProvider { get; }

        #endregion

        #region Implementation of interfaces

        public Task<object> GetViewAsync(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewMappingInfo, nameof(viewMappingInfo));
            Should.NotBeNull(metadata, nameof(metadata));
            if (ThreadDispatcher.IsOnMainThread)
                return Task.FromResult(GetViewImpl(viewMappingInfo, metadata));
            var handler = new ViewHandler(this, viewMappingInfo, metadata);
            ThreadDispatcher.Execute(handler, ThreadExecutionMode.Main, null);
            return handler.Task;
        }

        public Task InitializeViewAsync(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            if (ThreadDispatcher.IsOnMainThread)
            {
                InitializeViewImpl(view, viewModel, metadata);
                return Default.CompletedTask;
            }
            var handler = new ViewHandler(this, view, viewModel, metadata, false);
            ThreadDispatcher.Execute(handler, ThreadExecutionMode.Main, null);
            return handler.Task;
        }

        public Task CleanupViewAsync(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            object view = viewModel.GetCurrentView<object>(false);
            if (view == null)
                return Default.CompletedTask;
            var handler = new ViewHandler(this, view, viewModel, metadata, true);
            ThreadDispatcher.Execute(handler, ThreadExecutionMode.Main, null, metadata: Default.AlwaysAsyncThreadDispatcherContext);
            return handler.Task;
        }

        #endregion

        #region Methods

        protected virtual object GetViewInternal(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
        {
            return ServiceProvider.GetService(viewMappingInfo.ViewType);
        }

        protected virtual void InitializeViewInternal(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            view = MugenExtensions.GetUnderlyingView<object>(view);
            SetViewInternal(view, viewModel, metadata);
            var viewProperty = GetViewProperty(viewModel.GetType());
            if (viewProperty == null)
                return;

            if (!viewProperty.PropertyType.IsInstanceOfTypeUnified(view) && WrapperManager.CanWrap(view.GetType(), viewProperty.PropertyType, metadata))
                view = WrapperManager.Wrap(view, viewProperty.PropertyType, metadata);
            viewProperty.SetValue(viewModel, view);
        }

        protected virtual void CleanupViewInternal(IViewModel viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            view = MugenExtensions.GetUnderlyingView<object>(view);
            SetViewInternal(view, null, metadata);
            viewModel.Metadata.Remove(ViewModelMetadata.View);
            GetViewProperty(viewModel.GetType())?.SetValue(viewModel, null);
            viewModel.Unsubscribe(view);
        }

        protected virtual void OnViewCreated(object view, IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnViewCreated(this, view, viewMappingInfo, metadata);
            }
        }

        protected virtual void OnViewInitialized(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnViewInitialized(this, view, viewModel, metadata);
            }
        }

        protected virtual void OnViewCleared(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnViewCleared(this, view, viewModel, metadata);
            }
        }

        private object GetViewImpl(IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
        {
            var view = GetViewInternal(viewMappingInfo, metadata);
            if (Tracer.CanTrace(TraceLevel.Information))
                Tracer.Warn(MessageConstants.ViewCreatedFormat2, view.GetType(), viewMappingInfo.ViewModelType);
            OnViewCreated(view, viewMappingInfo, metadata);
            return view;
        }

        private void InitializeViewImpl(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            var oldView = viewModel.GetCurrentView<object>();
            if (ReferenceEquals(oldView, MugenExtensions.GetUnderlyingView<object>(view)))
                return;

            if (oldView != null)
                CleanupViewImpl(oldView, viewModel, metadata);

            InitializeViewInternal(view, viewModel, metadata);
            OnViewInitialized(view, viewModel, metadata);
        }

        private void CleanupViewImpl(object view, IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            CleanupViewInternal(viewModel, view, metadata);
            OnViewCleared(view, viewModel, metadata);
        }

        private void SetViewInternal(object view, IViewModel? viewModel, IReadOnlyMetadataContext metadata)
        {
            if (view == null)
                return;
            if (viewModel != null)
            {
                viewModel.Metadata.Set(ViewModelMetadata.View, view);
                viewModel.Subscribe(view);
            }

            DataContextProvider.SetDataContext(view, viewModel, metadata);
            GetViewModelPropertySetter(view.GetType())?.SetValue(view, viewModel);
            if (viewModel == null)
                (view as ICleanableView)?.Cleanup(metadata);
            else
                (view as IInitializableView)?.Initialize(viewModel, metadata);
        }

        protected static PropertyInfo GetViewModelPropertySetter(Type viewType)
        {
            lock (ViewToViewModelInterface)
            {
                if (!ViewToViewModelInterface.TryGetValue(viewType, out var result))
                {
                    foreach (var @interface in viewType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewModelAwareView<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view", "IViewModelAwareView<>", viewType);
                        result = @interface.GetPropertyUnified(nameof(IViewModelAwareView<IViewModel>.ViewModel), MemberFlags.InstancePublic);
                    }

                    ViewToViewModelInterface[viewType] = result;
                }

                return result;
            }
        }

        protected static PropertyInfo GetViewProperty(Type viewModelType)
        {
            lock (ViewModelToViewInterface)
            {
                if (!ViewModelToViewInterface.TryGetValue(viewModelType, out var result))
                {
                    foreach (var @interface in viewModelType.GetInterfacesUnified().Where(type => type.IsGenericTypeUnified()))
                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewAwareViewModel<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view model", "IViewAwareViewModel<>", viewModelType);
                        result = @interface.GetPropertyUnified(nameof(IViewAwareViewModel<object>.View), MemberFlags.InstancePublic);
                    }

                    ViewModelToViewInterface[viewModelType] = result;
                }

                return result;
            }
        }

        #endregion

        #region Nested types

        private sealed class ViewHandler : TaskCompletionSource<object>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly IReadOnlyMetadataContext _metadata;
            private readonly object _view;
            private readonly ViewManager _viewManager;
            private readonly IViewMappingInfo _viewMappingInfo;
            private readonly IViewModel _viewModel;
            private readonly int _state;

            #endregion

            #region Constructors

            public ViewHandler(ViewManager viewManager, IViewMappingInfo viewMappingInfo, IReadOnlyMetadataContext metadata)
            {
                _viewManager = viewManager;
                _viewMappingInfo = viewMappingInfo;
                _metadata = metadata;
            }

            public ViewHandler(ViewManager viewManager, object view, IViewModel viewModel, IReadOnlyMetadataContext metadata, bool isCleanup)
            {
                _state = isCleanup ? 2 : 1;
                _viewManager = viewManager;
                _view = view;
                _viewModel = viewModel;
                _metadata = metadata;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                try
                {
                    if (_state == 0)
                    {
                        var result = _viewManager.GetViewImpl(_viewMappingInfo, _metadata);
                        TrySetResult(result);
                    }
                    else if (_state == 1)
                        _viewManager.InitializeViewImpl(_view, _viewModel, _metadata);
                    else
                        _viewManager.CleanupViewImpl(_view, _viewModel, _metadata);
                }
                catch (Exception e)
                {
                    this.TrySetExceptionEx(e);
                }
            }

            #endregion
        }

        #endregion
    }
}