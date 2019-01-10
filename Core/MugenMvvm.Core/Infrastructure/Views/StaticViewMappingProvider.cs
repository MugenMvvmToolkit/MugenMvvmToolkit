using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views.Infrastructure;

namespace MugenMvvm.Infrastructure.Views
{
    public class StaticViewMappingProvider : IViewMappingProvider
    {
        #region Fields

        private readonly Dictionary<Type, Dictionary<string, IViewMappingInfo>> _viewModelToMapping;
        private readonly Dictionary<Type, List<IViewMappingInfo>> _viewTypeToMapping;

        #endregion

        #region Constructors

        public StaticViewMappingProvider()
        {
            _viewTypeToMapping = new Dictionary<Type, List<IViewMappingInfo>>(MemberInfoComparer.Instance);
            _viewModelToMapping = new Dictionary<Type, Dictionary<string, IViewMappingInfo>>(MemberInfoComparer.Instance);
        }

        #endregion

        #region Properties

        public IEnumerable<IViewMappingInfo> Mappings
        {
            get
            {
                EnsureInitialized();
                return _viewModelToMapping.Values.SelectMany(items => items.Values);
            }
        }

        protected object Locker => _viewModelToMapping;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByView(Type viewType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            Should.NotBeNull(metadata, nameof(metadata));
            EnsureInitialized();
            return TryGetMappingsByViewInternal(viewType, metadata);
        }

        public IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByViewModel(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            EnsureInitialized();
            return TryGetMappingsByViewModelInternal(viewModelType, metadata);
        }

        public IViewMappingInfo? TryGetMappingByViewModel(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            Should.NotBeNull(metadata, nameof(metadata));
            EnsureInitialized();
            return TryGetMappingByViewModelInternal(viewModelType, metadata);
        }

        #endregion

        #region Methods

        public void AddMapping(IViewMappingInfo mappingInfo, bool throwOnError = true, bool rewrite = false)
        {
            if (!_viewTypeToMapping.TryGetValue(mappingInfo.ViewType, out var list))
            {
                list = new List<IViewMappingInfo>();
                _viewTypeToMapping[mappingInfo.ViewType] = list;
            }

            list.Add(mappingInfo);

            if (!_viewModelToMapping.TryGetValue(mappingInfo.ViewModelType, out var value))
            {
                value = new Dictionary<string, IViewMappingInfo>();
                _viewModelToMapping[mappingInfo.ViewModelType] = value;
            }

            var name = mappingInfo.Name ?? string.Empty;
            if (value.TryGetValue(name, out var item))
            {
                if (throwOnError)
                    throw ExceptionManager.DuplicateViewMapping(item.ViewType, item.ViewModelType, item.Name);
                if (!rewrite)
                    return;
            }

            value[name] = mappingInfo;
        }

        protected virtual IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByViewInternal(Type viewType, IReadOnlyMetadataContext metadata)
        {
            _viewTypeToMapping.TryGetValue(viewType, out var item);
            return item;
        }

        protected virtual IReadOnlyCollection<IViewMappingInfo>? TryGetMappingsByViewModelInternal(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            if (!_viewModelToMapping.TryGetValue(viewModelType, out var value))
            {
                if (viewModelType.IsGenericTypeUnified())
                {
                    viewModelType = viewModelType.GetGenericTypeDefinition();
                    if (viewModelType != null)
                        _viewModelToMapping.TryGetValue(viewModelType, out value);
                }
            }

            if (value == null)
                return null;
            return value.Values.ToReadOnlyCollection();
        }

        protected virtual IViewMappingInfo? TryGetMappingByViewModelInternal(Type viewModelType, IReadOnlyMetadataContext metadata)
        {
            if (!_viewModelToMapping.TryGetValue(viewModelType, out var value))
            {
                if (viewModelType.IsGenericTypeUnified())
                {
                    viewModelType = viewModelType.GetGenericTypeDefinition();
                    if (viewModelType != null)
                        _viewModelToMapping.TryGetValue(viewModelType, out value);
                }
            }

            if (value == null)
                return null;

            var viewName = metadata.Get(NavigationMetadata.ViewName) ?? metadata.Get(NavigationMetadata.ViewModel)?.Metadata.Get(NavigationMetadata.ViewName) ?? string.Empty;
            if (!value.TryGetValue(viewName, out var mapping))
            {
                if (viewName != string.Empty)
                    value.TryGetValue(string.Empty, out mapping);
            }

            return mapping;
        }

        protected virtual void EnsureInitialized()
        {
        }

        #endregion
    }
}