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

        public ViewInitializer(ThreadExecutionMode initializeExecutionMode, ThreadExecutionMode cleanupExecutionMode, string id, Type viewType, Type viewModelType,
            IReadOnlyMetadataContext metadata, IViewManager? viewManager = null, IThreadDispatcher? threadDispatcher = null, IMetadataContextProvider? metadataContextProvider = null)
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

        public bool HasMetadata => Metadata.Count != 0;

        public IReadOnlyMetadataContext Metadata { get; }

        public Type ViewType { get; }

        public Type ViewModelType { get; }

        public string Id { get; }

        private ThreadExecutionMode InitializeExecutionMode { get; }

        private ThreadExecutionMode CleanupExecutionMode { get; }

        #endregion

        #region Implementation of interfaces

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return new ViewManagerInitializerHandler(this, viewModel, null, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModel, nameof(viewModel), ViewModelType);
            Should.BeOfType(view, nameof(view), ViewType);
            return new ViewManagerInitializerHandler(this, viewModel, view, metadata).Task;
        }

        public Task<IViewInitializerResult> InitializeAsync(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            return new ViewManagerInitializerHandler(this, null, view, metadata).Task;
        }

        public Task<IReadOnlyMetadataContext> CleanupAsync(IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewInfo, nameof(viewInfo));
            Should.NotBeNull(viewModel, nameof(viewModel));
            return new CleanupHandler(this, viewInfo, viewModel, metadata).Task;
        }

        #endregion

        #region Methods

        private IViewInitializerResult Initialize(IViewInitializer initializer, IViewModelBase? viewModel, object? view, IMetadataContext metadata)
        {
            var components = _viewManager.DefaultIfNull().GetComponents<IViewInitializerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryInitialize(initializer, viewModel, view, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowObjectNotInitialized(_viewManager.DefaultIfNull(), components);
            return null;
        }

        private IReadOnlyMetadataContext Cleanup(IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = _viewManager.DefaultIfNull().GetComponents<IViewInitializerComponent>(metadata);
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryCleanup(initializer, viewInfo, viewModel, metadata);
                if (result != null)
                    return result;
            }

            ExceptionManager.ThrowObjectNotInitialized(_viewManager.DefaultIfNull(), components);
            return null;
        }

        #endregion

        #region Nested types

        private sealed class ViewManagerInitializerHandler : TaskCompletionSource<IViewInitializerResult>, IThreadDispatcherHandler<object?>
        {
            #region Fields

            private readonly ViewInitializer _initializer;
            private readonly IMetadataContext _metadata;
            private readonly object? _view;
            private readonly IViewModelBase? _viewModel;

            #endregion

            #region Constructors

            public ViewManagerInitializerHandler(ViewInitializer initializer, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext? metadata)
            {
                _initializer = initializer;
                _viewModel = viewModel;
                _view = view;
                _metadata = metadata.ToNonReadonly(initializer, initializer._metadataContextProvider);
                initializer._threadDispatcher.DefaultIfNull().Execute(initializer.InitializeExecutionMode, this);
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? _)
            {
                try
                {
                    TrySetResult(_initializer.Initialize(_initializer, _viewModel, _view, _metadata));
                }
                catch (Exception e)
                {
                    TrySetException(e);
                }
            }

            #endregion
        }

        private sealed class CleanupHandler : TaskCompletionSource<IReadOnlyMetadataContext>, IThreadDispatcherHandler<object?>
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
                _metadata = metadata.ToNonReadonly(initializer, initializer._metadataContextProvider);
                initializer._threadDispatcher.DefaultIfNull().Execute(initializer.CleanupExecutionMode, this);
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? _)
            {
                try
                {
                    TrySetResult(_initializer.Cleanup(_initializer, _viewInfo, _viewModel, _metadata));
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