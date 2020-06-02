using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewModelViewMappingProvider : IViewModelViewMappingProviderComponent, IHasPriority
    {
        #region Fields

        private readonly List<MappingInfo> _mappings;

        #endregion

        #region Constructors

        public ViewModelViewMappingProvider()
        {
            _mappings = new List<MappingInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.MappingProvider;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IViewModelViewMapping, IReadOnlyList<IViewModelViewMapping>> TryGetMappings<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            IViewModelBase? vm;
            object? view;
            Type? type;
            string? id;
            if (Default.IsValueType<TRequest>())
            {
                if (typeof(TRequest) != typeof(ViewModelViewRequest))
                    return default;
                var r = MugenExtensions.CastGeneric<TRequest, ViewModelViewRequest>(request);
                if (r.IsEmpty)
                    return default;

                vm = r.ViewModel;
                view = r.View;
                type = null;
                id = null;
            }
            else
            {
                vm = request as IViewModelBase;
                if (vm == null)
                    view = request;
                else
                    view = null;
                type = request as Type;
                id = request as string;
            }
            ItemOrList<IViewModelViewMapping, List<IViewModelViewMapping>> mappings = default;
            lock (_mappings)
            {
                for (var i = 0; i < _mappings.Count; i++)
                {
                    var mapping = _mappings[i];
                    if (id != null)
                    {
                        if (mapping.Mapping.Id == id)
                            mappings.Add(mapping.Mapping);
                    }
                    else if (vm != null && view != null)
                    {
                        if (mapping.IsValidViewModelType(vm.GetType(), metadata) && mapping.IsValidViewType(view.GetType(), metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else if (vm != null)
                    {
                        if (mapping.IsValidViewModelType(vm.GetType(), metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else if (type != null)
                    {
                        if (mapping.IsValidViewModelType(type, metadata) || mapping.IsValidViewType(type, metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else
                    {
                        if (mapping.IsValidViewType(view!.GetType(), metadata))
                            mappings.Add(mapping.Mapping);
                    }
                }
            }

            return mappings.Cast<IReadOnlyList<IViewModelViewMapping>>();
        }

        #endregion

        #region Methods

        public void AddMapping(Type viewModelType, Type viewType, bool exactlyEqual = false, string? name = null, string? id = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModelType, nameof(viewModelType), typeof(IViewModelBase));
            Should.NotBeNull(viewType, nameof(viewType));
            var mapping = new ViewModelViewMapping(id ?? $"{viewModelType.FullName}{viewType.FullName}{name}", viewType, viewModelType, metadata);
            AddMapping(mapping, exactlyEqual, name);
        }

        public void AddMapping(IViewModelViewMapping mapping, bool exactlyEqual = false, string? name = null)
        {
            Should.NotBeNull(mapping, nameof(mapping));
            var mappingInfo = new MappingInfo(mapping, exactlyEqual, name);
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
            private readonly string? _name;
            public readonly IViewModelViewMapping Mapping;

            #endregion

            #region Constructors

            public MappingInfo(IViewModelViewMapping mapping, bool exactlyEqual, string? name)
            {
                _exactlyEqual = exactlyEqual;
                _name = name;
                Mapping = mapping;
            }

            #endregion

            #region Methods

            public bool IsValidViewType(Type viewType, IReadOnlyMetadataContext? metadata)
            {
                if (_name != GetViewNameFromContext(metadata))
                    return false;

                if (_exactlyEqual)
                {
                    if (viewType == Mapping.ViewType)
                        return true;
                }
                else if (Mapping.ViewType.IsAssignableFrom(viewType))
                    return true;

                return false;
            }

            public bool IsValidViewModelType(Type viewModelType, IReadOnlyMetadataContext? metadata)
            {
                if (_name != GetViewNameFromContext(metadata))
                    return false;

                if (_exactlyEqual)
                {
                    if (viewModelType == Mapping.ViewModelType)
                        return true;
                }
                else if (Mapping.ViewModelType.IsAssignableFrom(viewModelType))
                    return true;

                return false;
            }

            private static string? GetViewNameFromContext(IReadOnlyMetadataContext? metadata)
            {
                return metadata?.Get(NavigationMetadata.ViewName) ?? (metadata?.Get(NavigationMetadata.Target) as IMetadataOwner<IReadOnlyMetadataContext>)?.Metadata.Get(NavigationMetadata.ViewName);
            }

            #endregion
        }

        #endregion
    }
}