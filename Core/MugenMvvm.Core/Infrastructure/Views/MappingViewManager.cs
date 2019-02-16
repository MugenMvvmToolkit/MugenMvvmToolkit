using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Views
{
    public class MappingViewManager : IChildViewManager
    {
        #region Fields

        private readonly List<MappingInfo> _mappings;
        private int _mappingIdCounter;

        protected static readonly IMetadataContextKey<Dictionary<int, IViewInfo>> ViewsMetadataKey;

        #endregion

        #region Constructors

        static MappingViewManager()
        {
            ViewsMetadataKey = MetadataContextKey.FromMember<Dictionary<int, IViewInfo>>(typeof(MappingViewManager), nameof(ViewsMetadataKey));
        }

        [Preserve(Conditional = true)]
        public MappingViewManager(IThreadDispatcher threadDispatcher, IViewModelDispatcher viewModelDispatcher, IViewDataContextProvider dataContextProvider,
            IServiceProvider serviceProvider)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(viewModelDispatcher, nameof(viewModelDispatcher));
            Should.NotBeNull(dataContextProvider, nameof(dataContextProvider));
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            ThreadDispatcher = threadDispatcher;
            ViewModelDispatcher = viewModelDispatcher;
            DataContextProvider = dataContextProvider;
            ServiceProvider = serviceProvider;
            _mappings = new List<MappingInfo>();
            InitializeExecutionMode = ThreadExecutionMode.Main;
            CleanupExecutionMode = ThreadExecutionMode.MainAsync;
        }

        #endregion

        #region Properties

        public virtual int Priority => 0;

        public ThreadExecutionMode InitializeExecutionMode { get; set; }

        public ThreadExecutionMode CleanupExecutionMode { get; set; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected IViewModelDispatcher ViewModelDispatcher { get; }

        protected IViewDataContextProvider DataContextProvider { get; }

        protected IServiceProvider ServiceProvider { get; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentViewManager, nameof(parentViewManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetViewsInternal(parentViewManager, viewModel, metadata);
        }

        public IReadOnlyList<IViewModelViewInitializer> GetInitializersByView(IParentViewManager parentViewManager, object view, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentViewManager, nameof(parentViewManager));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewInternal(parentViewManager, view, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(parentViewManager, nameof(parentViewManager));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetInitializersByViewModelInternal(parentViewManager, viewModel, metadata);
        }

        #endregion

        #region Methods

        public void AddMapping(Type viewModelType, Type viewType, bool exactlyEqual, string? name, IReadOnlyMetadataContext metadata)
        {
            Should.BeOfType(viewModelType, nameof(viewModelType), typeof(IViewModelBase));
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(metadata, nameof(metadata));
            var mappingInfo = new MappingInfo(Interlocked.Increment(ref _mappingIdCounter), metadata, null, null, viewModelType, viewType, exactlyEqual, name);
            lock (_mappings)
            {
                _mappings.Add(mappingInfo);
            }
        }

        public void AddMapping(Func<IViewModelBase, IReadOnlyMetadataContext, Type?> getViewType, Func<object, IReadOnlyMetadataContext, Type?> getViewModelType,
            IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(getViewType, nameof(getViewType));
            Should.NotBeNull(getViewModelType, nameof(getViewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            var mappingInfo = new MappingInfo(Interlocked.Increment(ref _mappingIdCounter), metadata, getViewModelType, getViewType, null, null, false, null);
            lock (_mappings)
            {
                _mappings.Add(mappingInfo);
            }
        }

        public void ClearMapping()
        {
            lock (_mappings)
            {
                _mappings.Clear();
            }
        }

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IParentViewManager parentViewManager, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            var list = viewModel.Metadata.Get(ViewsMetadataKey);
            if (list == null)
                return Default.EmptyArray<IViewInfo>();
            lock (list)
            {
                return list.Values.ToArray();
            }
        }

        protected virtual IReadOnlyList<IViewModelViewInitializer> GetInitializersByViewInternal(IParentViewManager parentViewManager, object view,
            IReadOnlyMetadataContext metadata)
        {
            var initializers = new List<IViewModelViewInitializer>();
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var viewModelType = _mappings[i].GetViewModelType(view, metadata);
                    if (viewModelType != null)
                        initializers.Add(new Initializer(this, parentViewManager, _mappings[i], viewModelType, view.GetType()));
                }
            }

            return initializers;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IParentViewManager parentViewManager, IViewModelBase viewModel,
            IReadOnlyMetadataContext metadata)
        {
            var initializers = new List<IViewInitializer>();
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var viewType = _mappings[i].GetViewType(viewModel, metadata);
                    if (viewType != null)
                        initializers.Add(new Initializer(this, parentViewManager, _mappings[i], viewModel.GetType(), viewType));
                }
            }

            return initializers;
        }

        protected virtual IViewManagerResult Initialize(IParentViewManager parentViewManager, IViewManagerInitializer initializer, int mappingId, IViewModelBase viewModel,
            object view, IReadOnlyMetadataContext metadata)
        {
            var views = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?)null, (object?)null, (context, vm, _) => new Dictionary<int, IViewInfo>());
            ViewInfo viewInfo;
            lock (views)
            {
                if (views.TryGetValue(mappingId, out var oldView))
                {
                    if (ReferenceEquals(oldView.View, view))
                        return new ViewManagerResult(viewModel, oldView, Default.MetadataContext);

                    CleanupInternal(parentViewManager, oldView, viewModel, metadata);
                }

                viewInfo = new ViewInfo(this, parentViewManager, mappingId, view, null);
                views[mappingId] = viewInfo;
            }

            ViewModelDispatcher.Subscribe(viewModel, view, ThreadExecutionMode.Main, metadata);
            DataContextProvider.SetDataContext(view, viewModel, metadata);
            (view as IInitializableView)?.Initialize(viewModel, viewInfo, metadata);
            parentViewManager.OnViewInitialized(viewModel, viewInfo, metadata);

            return new ViewManagerResult(viewModel, viewInfo, Default.MetadataContext);
        }

        protected virtual ICleanupViewManagerResult Cleanup(IParentViewManager parentViewManager, IViewInfo viewInfo, int mappingId, IViewModelBase viewModel,
            IReadOnlyMetadataContext metadata)
        {
            var views = viewModel.Metadata.Get(ViewsMetadataKey);
            if (views == null)
                return CleanupViewManagerResult.Empty;

            lock (views)
            {
                if (!views.Remove(mappingId))
                    return CleanupViewManagerResult.Empty;
            }

            CleanupInternal(parentViewManager, viewInfo, viewModel, metadata);
            return CleanupViewManagerResult.Empty;
        }

        protected virtual IViewModelBase GetViewModelForView(IParentViewManager parentViewManager, IViewModelViewInitializer initializer, object view,
            IReadOnlyMetadataContext metadata)
        {
            var vm = (IViewModelBase)ServiceProvider.GetService(initializer.ViewModelType);
            parentViewManager.OnViewModelCreated(vm, view, metadata);
            return vm;
        }

        protected virtual object GetViewForViewModel(IParentViewManager parentViewManager, IViewInitializer initializer, IViewModelBase viewModel,
            IReadOnlyMetadataContext metadata)
        {
            var view = (IViewModelBase)ServiceProvider.GetService(initializer.ViewType);
            parentViewManager.OnViewCreated(viewModel, view, metadata);
            return view;
        }

        private void CleanupInternal(IParentViewManager parentViewManager, IViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
        {
            ViewModelDispatcher.Unsubscribe(viewModel, viewInfo.View, metadata);
            DataContextProvider.SetDataContext(viewInfo.View, null, metadata);
            (viewInfo.View as ICleanableView)?.Cleanup(metadata);
            parentViewManager.OnViewCleared(viewModel, viewInfo, metadata);
        }

        #endregion

        #region Nested types

        private struct MappingInfo
        {
            #region Fields

            private bool _exactlyEqual;
            private Func<object, IReadOnlyMetadataContext, Type?>? _getViewModelType;
            private Func<IViewModelBase, IReadOnlyMetadataContext, Type?>? _getViewType;
            private string? _name;
            private Type? _viewModelType;
            private Type? _viewType;

            public readonly IReadOnlyMetadataContext Metadata;
            public readonly int Id;

            #endregion

            #region Constructors

            public MappingInfo(int id, IReadOnlyMetadataContext metadata, Func<object, IReadOnlyMetadataContext, Type?>? getViewModelType,
                Func<IViewModelBase, IReadOnlyMetadataContext, Type?>? getViewType,
                Type? viewModelType, Type? viewType, bool exactlyEqual, string? name)
            {
                Id = id;
                Metadata = metadata;
                _getViewModelType = getViewModelType;
                _getViewType = getViewType;
                _viewModelType = viewModelType;
                _viewType = viewType;
                _exactlyEqual = exactlyEqual;
                _name = name;
            }

            #endregion

            #region Methods

            public Type? GetViewModelType(object view, IReadOnlyMetadataContext metadata)
            {
                if (_getViewModelType != null)
                    return _getViewModelType(view, metadata);

                if (_name != GetViewNameFromContext(metadata))
                    return null;

                if (_exactlyEqual)
                {
                    if (view.GetType() == _viewType)
                        return _viewModelType;
                }
                else if (_viewType.IsInstanceOfTypeUnified(view))
                    return _viewModelType;

                return null;
            }

            public Type? GetViewType(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                if (_getViewType != null)
                    return _getViewType(viewModel, metadata);

                if (_name != GetViewNameFromContext(metadata))
                    return null;

                if (_exactlyEqual)
                {
                    if (viewModel.GetType() == _viewModelType)
                        return _viewType;
                }
                else if (_viewModelType.IsInstanceOfTypeUnified(viewModel))
                    return _viewType;

                return null;
            }

            private static string? GetViewNameFromContext(IReadOnlyMetadataContext metadata)
            {
                return metadata.Get(NavigationMetadata.ViewName) ?? metadata.Get(NavigationMetadata.ViewModel)?.Metadata.Get(NavigationMetadata.ViewName);
            }

            #endregion
        }

        private sealed class Initializer : IViewInitializer, IViewModelViewInitializer
        {
            #region Fields

            public readonly MappingInfo Mapping;
            public readonly IParentViewManager ParentViewManager;
            public readonly MappingViewManager ViewManager;

            #endregion

            #region Constructors

            public Initializer(MappingViewManager viewManager, IParentViewManager parentViewManager, MappingInfo mapping, Type viewModelType, Type viewType)
            {
                ViewManager = viewManager;
                ParentViewManager = parentViewManager;
                Mapping = mapping;
                ViewModelType = viewModelType;
                ViewType = viewType;
            }

            #endregion

            #region Properties

            public IReadOnlyMetadataContext Metadata => Mapping.Metadata;

            public Type ViewType { get; }

            public Type ViewModelType { get; }

            #endregion

            #region Implementation of interfaces

            public Task<IViewManagerResult> InitializeAsync(IViewModelBase viewModel, object view, IReadOnlyMetadataContext metadata)
            {
                Should.BeOfType(viewModel, nameof(viewModel), ViewModelType);
                Should.BeOfType(view, nameof(view), ViewType);
                Should.NotBeNull(metadata, nameof(metadata));
                return new InitializerHandler(this, viewModel, view, metadata, false).Task;
            }

            public Task<IViewManagerResult> InitializeAsync(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                Should.NotBeNull(viewModel, nameof(viewModel));
                Should.NotBeNull(metadata, nameof(metadata));
                return new InitializerHandler(this, viewModel, null, metadata, true).Task;
            }

            public Task<IViewManagerResult> InitializeAsync(object view, IReadOnlyMetadataContext metadata)
            {
                Should.NotBeNull(view, nameof(view));
                Should.NotBeNull(metadata, nameof(metadata));
                return new InitializerHandler(this, null, view, metadata, false).Task;
            }

            #endregion
        }

        private sealed class InitializerHandler : TaskCompletionSource<IViewManagerResult>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly Initializer _initializer;
            private readonly bool _isView;

            private readonly IReadOnlyMetadataContext _metadata;
            private object? _view;
            private IViewModelBase? _viewModel;

            #endregion

            #region Constructors

            public InitializerHandler(Initializer initializer, IViewModelBase? viewModel, object? view, IReadOnlyMetadataContext metadata, bool isView)
            {
                _initializer = initializer;
                _viewModel = viewModel;
                _view = view;
                _metadata = metadata;
                _isView = isView;
                initializer.ViewManager.ThreadDispatcher.Execute(this, initializer.ViewManager.InitializeExecutionMode, null, metadata: metadata);
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
                            _view = _initializer.ViewManager.GetViewForViewModel(_initializer.ParentViewManager, _initializer, _viewModel, _metadata);
                    }
                    else
                    {
                        if (_viewModel == null)
                            _viewModel = _initializer.ViewManager.GetViewModelForView(_initializer.ParentViewManager, _initializer, _view, _metadata);
                    }

                    var result = _initializer.ViewManager.Initialize(_initializer.ParentViewManager, _initializer, _initializer.Mapping.Id, _viewModel, _view, _metadata);
                    TrySetResult(result);
                }
                catch (Exception e)
                {
                    TrySetException(e);
                }
            }

            #endregion
        }

        private sealed class ViewInfo : IViewInfo
        {
            #region Fields

            private readonly int _mappingId;
            private readonly IParentViewManager _parentViewManager;
            private readonly MappingViewManager _viewManager;

            private CleanupHandler? _cleanupHandler;
            private IObservableMetadataContext? _metadata;

            #endregion

            #region Constructors

            public ViewInfo(MappingViewManager viewManager, IParentViewManager parentViewManager, int mappingId, object view, IObservableMetadataContext? metadata)
            {
                _viewManager = viewManager;
                _mappingId = mappingId;
                _parentViewManager = parentViewManager;
                _metadata = metadata;
                View = view;
            }

            #endregion

            #region Properties

            public IObservableMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        MugenExtensions.LazyInitialize(ref _metadata, new MetadataContext());
                    return _metadata;
                }
            }

            public object View { get; }

            #endregion

            #region Implementation of interfaces

            public Task<ICleanupViewManagerResult> CleanupAsync(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                if (_cleanupHandler == null && MugenExtensions.LazyInitialize(ref _cleanupHandler, new CleanupHandler(this, viewModel, metadata)))
                    _viewManager.ThreadDispatcher.Execute(_cleanupHandler, _viewManager.CleanupExecutionMode, null, metadata: _metadata);
                return _cleanupHandler.Task;
            }

            #endregion

            #region Methods

            public ICleanupViewManagerResult Cleanup(IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                var result = _viewManager.Cleanup(_parentViewManager, this, _mappingId, viewModel, metadata);
                _metadata?.Clear();
                return result;
            }

            #endregion
        }

        private sealed class CleanupHandler : TaskCompletionSource<ICleanupViewManagerResult>, IThreadDispatcherHandler
        {
            #region Fields

            private readonly IReadOnlyMetadataContext _metadata;
            private readonly ViewInfo _viewInfo;
            private readonly IViewModelBase _viewModel;

            #endregion

            #region Constructors

            public CleanupHandler(ViewInfo viewInfo, IViewModelBase viewModel, IReadOnlyMetadataContext metadata)
            {
                _viewInfo = viewInfo;
                _viewModel = viewModel;
                _metadata = metadata;
            }

            #endregion

            #region Implementation of interfaces

            public void Execute(object? state)
            {
                try
                {
                    TrySetResult(_viewInfo.Cleanup(_viewModel, _metadata));
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