using System;
using System.Collections.Generic;
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

namespace MugenMvvm.Views.Components
{
    public sealed class ViewMappingProvider : IViewMappingProviderComponent, IHasPriority
    {
        #region Fields

        private readonly List<MappingInfo> _mappings;

        #endregion

        #region Constructors

        public ViewMappingProvider()
        {
            _mappings = new List<MappingInfo>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.MappingProvider;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings(IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            var vm = MugenExtensions.TryGetViewModelView(request, out object? view);
            var type = view as Type;
            var id = view as string;

            var mappings = ItemOrListEditor.Get<IViewMapping>();
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

            return mappings.ToItemOrList<IReadOnlyList<IViewMapping>>();
        }

        #endregion

        #region Methods

        public void AddMapping(Type viewModelType, Type viewType, bool exactlyEqual = true, string? name = null, string? id = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.BeOfType(viewModelType, typeof(IViewModelBase), nameof(viewModelType));
            Should.NotBeNull(viewType, nameof(viewType));
            var mapping = new ViewMapping(id ?? $"{viewModelType.Name}{viewType.Name}{name}", viewModelType, viewType, metadata);
            AddMapping(mapping, exactlyEqual, name);
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

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MappingInfo
        {
            #region Fields

            private readonly bool _exactlyEqual;
            private readonly string? _name;
            public readonly IViewMapping Mapping;

            #endregion

            #region Constructors

            public MappingInfo(IViewMapping mapping, bool exactlyEqual, string? name)
            {
                _exactlyEqual = exactlyEqual;
                _name = name;
                Mapping = mapping;
            }

            #endregion

            #region Methods

            public bool IsValidViewType(Type viewType, object? target, IReadOnlyMetadataContext? metadata)
            {
                if (_name != GetViewNameFromContext(target, metadata))
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

            public bool IsValidViewModelType(Type viewModelType, object? target, IReadOnlyMetadataContext? metadata)
            {
                if (_name != GetViewNameFromContext(target, metadata))
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

            private static string? GetViewNameFromContext(object? target, IReadOnlyMetadataContext? metadata) =>
                metadata?.Get(NavigationMetadata.ViewName) ?? (target as IMetadataOwner<IReadOnlyMetadataContext>)?.Metadata.Get(NavigationMetadata.ViewName);

            #endregion
        }

        #endregion
    }
}