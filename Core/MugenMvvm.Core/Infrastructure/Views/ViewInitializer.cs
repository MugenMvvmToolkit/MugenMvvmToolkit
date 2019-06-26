using System;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Infrastructure.Views
{
    public sealed class ViewInitializer : IViewInitializer
    {
        #region Constructors

        public ViewInitializer(IThreadDispatcher threadDispatcher, IViewManager viewManager, ThreadExecutionMode initializeExecutionMode, ThreadExecutionMode cleanupExecutionMode,
            string id, Type viewType, Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(initializeExecutionMode, nameof(initializeExecutionMode));
            Should.NotBeNull(cleanupExecutionMode, nameof(cleanupExecutionMode));
            Should.NotBeNull(id, nameof(id));
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            Metadata = metadata;
            InitializeExecutionMode = initializeExecutionMode;
            CleanupExecutionMode = cleanupExecutionMode;
            ThreadDispatcher = threadDispatcher;
            ViewManager = viewManager;
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

        public IThreadDispatcher ThreadDispatcher { get; }

        public IViewManager ViewManager { get; }

        public ThreadExecutionMode InitializeExecutionMode { get; }

        public ThreadExecutionMode CleanupExecutionMode { get; }

        #endregion

        #region Implementation of interfaces

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return new ViewManagerInitializerHandler(this, viewModel, null, true, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            Should.BeOfType(viewModel, nameof(viewModel), ViewModelType);
            Should.BeOfType(view, nameof(view), ViewType);
            Should.NotBeNull(metadata, nameof(metadata));
            return new ViewManagerInitializerHandler(this, viewModel, view, false, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return new ViewManagerInitializerHandler(this, null, view, false, metadata).Task;
        }

        public Task<IReadOnlyMetadataContext> CleanupAsync(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return new CleanupHandler(this, viewInfo, viewModel, metadata).Task;
        }

        #endregion

        #region Methods

        private object GetViewForViewModel(ViewInitializer initializer, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var view = (components[i] as IViewProviderComponent)?.TryGetViewForViewModel(initializer, viewModel, metadata);
                if (view != null)
                    return view;
            }

            ExceptionManager.ThrowCannotGetComponent(ViewManager, typeof(IViewProviderComponent));
            return null;
        }

        private IViewModelBase GetViewModelForView(ViewInitializer initializer, object view, IReadOnlyMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var viewModel = (components[i] as IViewModelProviderViewManagerComponent)?.TryGetViewModelForView(initializer, view, metadata);
                if (viewModel != null)
                    return viewModel;
            }

            ExceptionManager.ThrowCannotGetComponent(ViewManager, typeof(IViewModelProviderViewManagerComponent));
            return null;
        }

        private IViewInitializerResult Initialize(ViewInitializer initializer, IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as IViewInitializerComponent)?.TryInitialize(initializer, viewModel, view, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowCannotGetComponent(ViewManager, typeof(IViewInitializerComponent));
            return null;
        }

        private IReadOnlyMetadataContext Cleanup(ViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var components = ViewManager.GetComponents();
            for (var i = 0; i < components.Length; i++)
            {
                var result = (components[i] as IViewInitializerComponent)?.TryCleanup(initializer, viewInfo, viewModel, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowCannotGetComponent(ViewManager, typeof(IViewInitializerComponent));
            return null;
        }

        #endregion

        #region Nested types

        private sealed class ViewManagerInitializerHandler : TaskCompletionSource<IViewInitializerResult>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly ViewInitializer _initializer;
            private readonly bool _isView;
            private readonly IReadOnlyMetadataContext _metadata;
            private object? _view;
            private IViewModelBase? _viewModel;

            #endregion

            #region Constructors

            public ViewManagerInitializerHandler(ViewInitializer initializer, IViewModelBase? viewModel, object? view, bool isView, IReadOnlyMetadataContext metadata)
            {
                _initializer = initializer;
                _viewModel = viewModel;
                _view = view;
                _metadata = metadata;
                _isView = isView;
                initializer.ThreadDispatcher.Execute(this, initializer.InitializeExecutionMode, null, metadata: metadata);
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
            private readonly IReadOnlyMetadataContext _metadata;
            private readonly IViewInfo _viewInfo;
            private readonly IViewModelBase _viewModel;

            #endregion

            #region Constructors

            public CleanupHandler(ViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                _initializer = initializer;
                _viewInfo = viewInfo;
                _viewModel = viewModel;
                _metadata = metadata;
                initializer.ThreadDispatcher.Execute(this, initializer.CleanupExecutionMode, null, metadata: metadata);
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