using System;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views
{
    public sealed class ViewInitializer : IViewInitializer
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly IViewManager? _viewManager;

        #endregion

        #region Constructors

        public ViewInitializer(IThreadDispatcher? threadDispatcher, IViewManager? viewManager, IMetadataContextProvider? metadataContextProvider,
            ThreadExecutionMode initializeExecutionMode, ThreadExecutionMode cleanupExecutionMode, string id, Type viewType, Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(initializeExecutionMode, nameof(initializeExecutionMode));
            Should.NotBeNull(cleanupExecutionMode, nameof(cleanupExecutionMode));
            Should.NotBeNull(id, nameof(id));
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            Metadata = metadata;
            InitializeExecutionMode = initializeExecutionMode;
            CleanupExecutionMode = cleanupExecutionMode;
            _threadDispatcher = threadDispatcher;
            _viewManager = viewManager;
            _metadataContextProvider = metadataContextProvider;
            Id = id;
            ViewType = viewType;
            ViewModelType = viewModelType;
        }

        #endregion

        #region Properties

        public bool HasMetadata => true;

        public IReadOnlyMetadataContext Metadata { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }

        public string Id { get; }

        private IThreadDispatcher ThreadDispatcher => _threadDispatcher.ServiceIfNull();

        private IViewManager ViewManager => _viewManager.ServiceIfNull();

        private IMetadataContextProvider MetadataContextProvider => _metadataContextProvider.ServiceIfNull();

        private ThreadExecutionMode InitializeExecutionMode { get; }

        private ThreadExecutionMode CleanupExecutionMode { get; }

        #endregion

        #region Implementation of interfaces

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return new ViewManagerInitializerHandler(this, viewModel, null, true, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModel, nameof(viewModel), ViewModelType);
            Should.BeOfType(view, nameof(view), ViewType);
            return new ViewManagerInitializerHandler(this, viewModel, view, false, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            return new ViewManagerInitializerHandler(this, null, view, false, metadata).Task;
        }

        public Task<IReadOnlyMetadataContext> CleanupAsync(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(viewModel, nameof(viewModel));
            return new CleanupHandler(this, viewInfo, viewModel, metadata).Task;
        }

        #endregion

        #region Methods

        private object GetViewForViewModel(ViewInitializer initializer, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var view = (components[i] as IViewProviderComponent)?.TryGetViewForViewModel(initializer, viewModel, metadata);
                if (view != null)
                    return view;
            }

            ExceptionManager.ThrowObjectNotInitialized(ViewManager, typeof(IViewProviderComponent).Name);
            return null!;
        }

        private IViewModelBase GetViewModelForView(ViewInitializer initializer, object view, IMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = (components[i] as IViewModelProviderViewManagerComponent)?.TryGetViewModelForView(initializer, view, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            ExceptionManager.ThrowObjectNotInitialized(ViewManager, typeof(IViewModelProviderViewManagerComponent).Name);
            return null!;
        }

        private IViewInitializerResult Initialize(ViewInitializer initializer, IViewModelBase viewModel, object view, IMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as IViewInitializerComponent)?.TryInitialize(initializer, viewModel, view, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowObjectNotInitialized(ViewManager, typeof(IViewInitializerComponent).Name);
            return null!;
        }

        private IReadOnlyMetadataContext Cleanup(ViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as IViewInitializerComponent)?.TryCleanup(initializer, viewInfo, viewModel, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowObjectNotInitialized(ViewManager, typeof(IViewInitializerComponent).Name);
            return null!;
        }

        #endregion

        #region Nested types

        private sealed class ViewManagerInitializerHandler : TaskCompletionSource<IViewInitializerResult>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly ViewInitializer _initializer;
            private readonly bool _isView;
            private readonly IMetadataContext _metadata;
            private object? _view;
            private IViewModelBase? _viewModel;

            #endregion

            #region Constructors

            public ViewManagerInitializerHandler(ViewInitializer initializer, IViewModelBase? viewModel, object? view, bool isView, IReadOnlyMetadataContext? metadata)
            {
                _initializer = initializer;
                _viewModel = viewModel;
                _view = view;
                _metadata = metadata.ToNonReadonly(initializer, initializer.MetadataContextProvider);
                _isView = isView;
                initializer.ThreadDispatcher.Execute(initializer.InitializeExecutionMode, this);
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                try
                {
                    if (_isView)
                    {
                        if (_view == null)
                            _view = _initializer.GetViewForViewModel(_initializer, _viewModel!, _metadata);
                    }
                    else
                    {
                        if (_viewModel == null)
                            _viewModel = _initializer.GetViewModelForView(_initializer, _view!, _metadata);
                    }

                    var result = _initializer.Initialize(_initializer, _viewModel!, _view!, _metadata);
                    TrySetResult(result);
                }
                catch (Exception e)
                {
                    TrySetException(e);
                }
            }

            #endregion
        }

        private sealed class CleanupHandler : TaskCompletionSource<IReadOnlyMetadataContext>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly ViewInitializer _initializer;
            private readonly IMetadataContext _metadata;
            private readonly IViewInfo _viewInfo;
            private readonly IViewModelBase _viewModel;

            #endregion

            #region Constructors

            public CleanupHandler(ViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
            {
                _initializer = initializer;
                _viewInfo = viewInfo;
                _viewModel = viewModel;
                _metadata = metadata.ToNonReadonly(initializer, initializer.MetadataContextProvider);
                initializer.ThreadDispatcher.Execute(initializer.CleanupExecutionMode, this);
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                try
                {
                    var result = _initializer.Cleanup(_initializer, _viewInfo, _viewModel, _metadata);
                    TrySetResult(result);
                }
                catch (Exception e)
                {
                    TrySetException(e);
                }
            }

            #endregion
        }

        #endregion
    }
}