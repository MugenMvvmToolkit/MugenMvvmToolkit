using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewMappingProviderComponent : IViewModelViewMappingProviderComponent, IHasPriority
    {
        #region Fields

        private readonly List<MappingInfo> _mappings;

        #endregion

        #region Constructors

        public ViewModelViewMappingProviderComponent()
        {
            _mappings = new List<MappingInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.MappingProvider;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewModelViewMapping>? TryGetMappingByView(object view, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(view, nameof(view));
            List<IViewModelViewMapping>? mappings = null;
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var mapping = _mappings[i];
                    var viewModelType = mapping.GetViewModelType(view, metadata);
                    if (viewModelType == null)
                        continue;
                    if (mappings == null)
                        mappings = new List<IViewModelViewMapping>(2);
                    mappings.Add(new ViewModelViewMapping(mapping.Id, view.GetType(), viewModelType, mapping.Metadata));
                }
            }

            return mappings;
        }

        public IReadOnlyList<IViewModelViewMapping>? TryGetMappingByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            List<IViewModelViewMapping>? mappings = null;
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var mapping = _mappings[i];
                    var viewType = mapping.GetViewType(viewModel, metadata);
                    if (viewType == null)
                        continue;
                    if (mappings == null)
                        mappings = new List<IViewModelViewMapping>(2);
                    mappings.Add(new ViewModelViewMapping(mapping.Id, viewType, viewModel.GetType(), mapping.Metadata));
                }
            }

            return mappings;
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
                Id = "map-" + id.ToString(CultureInfo.InvariantCulture);
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
                else if (_viewType != null && _viewType.IsInstanceOfType(view))
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
                else if (_viewModelType != null && _viewModelType.IsInstanceOfType(viewModel))
                    return _viewType;

                return null;
            }

            private static string? GetViewNameFromContext(IReadOnlyMetadataContext? metadata)
            {
                return metadata?.Get(NavigationMetadata.ViewName!) ?? metadata?.Get(NavigationMetadata.ViewModel!)?.Metadata.Get(NavigationMetadata.ViewName!);
            }

            #endregion
        }

        #endregion
    }
}