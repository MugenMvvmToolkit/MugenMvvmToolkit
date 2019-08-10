using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class MappingViewInitializerProviderComponent : AttachableComponentBase<IViewManager>, IViewInitializerProviderComponent
    {
        #region Fields

        private readonly List<MappingInfo> _mappings;
        private readonly IThreadDispatcher? _threadDispatcher;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MappingViewInitializerProviderComponent(IThreadDispatcher? threadDispatcher = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _mappings = new List<MappingInfo>();
            InitializeExecutionMode = ThreadExecutionMode.Main;
            CleanupExecutionMode = ThreadExecutionMode.MainAsync;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public ThreadExecutionMode InitializeExecutionMode { get; set; }

        public ThreadExecutionMode CleanupExecutionMode { get; set; }

        #endregion

        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(view, nameof(view));
            var initializers = new List<IViewInitializer>();
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var mapping = _mappings[i];
                    var viewModelType = mapping.GetViewModelType(view, metadata);
                    if (viewModelType != null)
                    {
                        initializers.Add(new ViewInitializer(_threadDispatcher, Owner, _metadataContextProvider, InitializeExecutionMode, CleanupExecutionMode, mapping.Id,
                              view.GetType(), viewModelType, mapping.Metadata));
                    }
                }
            }

            return initializers;
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var initializers = new List<IViewInitializer>();
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var mapping = _mappings[i];
                    var viewType = mapping.GetViewType(viewModel, metadata);
                    if (viewType != null)
                    {
                        initializers.Add(new ViewInitializer(_threadDispatcher, Owner, _metadataContextProvider, InitializeExecutionMode, CleanupExecutionMode, mapping.Id,
                            viewType, viewModel.GetType(), mapping.Metadata));
                    }
                }
            }

            return initializers;
        }

        #endregion

        #region Methods

        public void AddMapping(Type viewModelType, Type viewType, bool exactlyEqual, string? name, IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModelType, nameof(viewModelType), typeof(IViewModelBase));
            Should.NotBeNull(viewType, nameof(viewType));
            var mappingInfo = new MappingInfo(Default.NextCounter(), metadata, null, null, viewModelType, viewType, exactlyEqual, name);
            lock (_mappings)
            {
                _mappings.Add(mappingInfo);
            }
        }

        public void AddMapping(Func<IViewModelBase, IReadOnlyMetadataContext?, Type?> getViewType, Func<object, IReadOnlyMetadataContext?, Type?> getViewModelType, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(getViewType, nameof(getViewType));
            Should.NotBeNull(getViewModelType, nameof(getViewModelType));
            var mappingInfo = new MappingInfo(Default.NextCounter(), metadata, getViewModelType, getViewType, null, null, false, null);
            lock (_mappings)
            {
                _mappings.Add(mappingInfo);
            }
        }

        public void ClearMappings()
        {
            lock (_mappings)
            {
                _mappings.Clear();
            }
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MappingInfo
        {
            #region Fields

            private readonly bool _exactlyEqual;
            private readonly Func<object, IReadOnlyMetadataContext?, Type?>? _getViewModelType;
            private readonly Func<IViewModelBase, IReadOnlyMetadataContext?, Type?>? _getViewType;
            private readonly string? _name;
            private readonly Type? _viewModelType;
            private readonly Type? _viewType;

            public readonly IReadOnlyMetadataContext Metadata;
            public readonly string Id;

            #endregion

            #region Constructors

            public MappingInfo(int id, IReadOnlyMetadataContext? metadata, Func<object, IReadOnlyMetadataContext?, Type?>? getViewModelType,
                Func<IViewModelBase, IReadOnlyMetadataContext?, Type?>? getViewType, Type? viewModelType, Type? viewType, bool exactlyEqual, string? name)
            {
                Id = "map-" + id;
                Metadata = metadata.DefaultIfNull();
                _getViewModelType = getViewModelType;
                _getViewType = getViewType;
                _viewModelType = viewModelType;
                _viewType = viewType;
                _exactlyEqual = exactlyEqual;
                _name = name;
            }

            #endregion

            #region Methods

            public Type? GetViewModelType(object view, IReadOnlyMetadataContext? metadata)
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
                else if (_viewType!.IsInstanceOfTypeUnified(view))
                    return _viewModelType;

                return null;
            }

            public Type? GetViewType(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
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
                else if (_viewModelType!.IsInstanceOfTypeUnified(viewModel))
                    return _viewType;

                return null;
            }

            private static string? GetViewNameFromContext(IReadOnlyMetadataContext? metadata)
            {
                return metadata?.Get(NavigationMetadata.ViewName) ?? metadata?.Get(NavigationMetadata.ViewModel)?.Metadata.Get(NavigationMetadata.ViewName);
            }

            #endregion
        }

        #endregion
    }
}