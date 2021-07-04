using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Collections;
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
    public sealed class ViewMappingProvider : IViewMappingProviderComponent, IHasPriority
    {
        private readonly List<MappingInfo> _mappings;

        public ViewMappingProvider()
        {
            _mappings = new List<MappingInfo>();
        }

        public int Priority { get; init; } = ViewComponentPriority.MappingProvider;

        public IViewMapping AddMapping(Type viewModelType, Type viewType, bool exactlyEqual = true, string? name = null, string? id = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModelType, typeof(IViewModelBase), nameof(viewModelType));
            Should.NotBeNull(viewType, nameof(viewType));
            var mapping = new ViewMapping(id ?? $"{viewModelType.Name}{viewType.Name}{name}", viewModelType, viewType, metadata);
            AddMapping(mapping, exactlyEqual, name);
            return mapping;
        }

        public void AddMapping(IViewMapping mapping, bool exactlyEqual = true, string? name = null)
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

        public ItemOrIReadOnlyList<IViewMapping> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var vm = MugenExtensions.TryGetViewModelView(request, out object? view);
            var type = view as Type;
            var id = view as string;

            var mappings = new ItemOrListEditor<IViewMapping>(2);
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
                        if (mapping.IsValidViewModelType(vm.GetType(), vm, metadata) && mapping.IsValidViewType(type ?? view.GetType(), view, metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else if (vm != null)
                    {
                        if (mapping.IsValidViewModelType(vm.GetType(), vm, metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else if (type != null)
                    {
                        if (mapping.IsValidViewModelType(type, null, metadata) || mapping.IsValidViewType(type, null, metadata))
                            mappings.Add(mapping.Mapping);
                    }
                    else if (view != null)
                    {
                        if (mapping.IsValidViewType(view.GetType(), view, metadata))
                            mappings.Add(mapping.Mapping);
                    }
                }
            }

            return mappings.ToItemOrList();
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MappingInfo
        {
            private readonly bool _exactlyEqual;
            private readonly string? _name;
            public readonly IViewMapping Mapping;

            public MappingInfo(IViewMapping mapping, bool exactlyEqual, string? name)
            {
                _exactlyEqual = exactlyEqual;
                _name = name;
                Mapping = mapping;
            }

            public bool IsValidViewType(Type viewType, object? target, IReadOnlyMetadataContext? metadata) => IsValidType(Mapping.ViewType, viewType, target, metadata);

            public bool IsValidViewModelType(Type viewModelType, object? target, IReadOnlyMetadataContext? metadata) =>
                IsValidType(Mapping.ViewModelType, viewModelType, target, metadata);

            private bool IsValidType(Type mappingType, Type requestedType, object? target, IReadOnlyMetadataContext? metadata)
            {
                if (_name != GetViewNameFromContext(target, metadata))
                    return false;

                if (mappingType.IsGenericTypeDefinition)
                {
                    if (requestedType.IsGenericType && mappingType == requestedType.GetGenericTypeDefinition())
                        return true;
                }
                else if (mappingType == requestedType)
                    return true;

                return !_exactlyEqual && mappingType.IsAssignableFromGeneric(requestedType);
            }

            private static string? GetViewNameFromContext(object? target, IReadOnlyMetadataContext? metadata) =>
                metadata?.Get(NavigationMetadata.ViewName) ?? (target as IMetadataOwner<IReadOnlyMetadataContext>)?.Metadata.Get(NavigationMetadata.ViewName);
        }
    }
}