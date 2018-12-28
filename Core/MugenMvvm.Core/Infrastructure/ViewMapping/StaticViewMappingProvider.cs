using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.ViewMapping;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.ViewMapping
{
    public class StaticViewMappingProvider : IViewMappingProvider
    {
        #region Fields

        private readonly Dictionary<Type, Dictionary<string, IViewMappingItem>> _viewModelToMapping;
        private readonly Dictionary<Type, List<IViewMappingItem>> _viewTypeToMapping;

        #endregion

        #region Constructors

        public StaticViewMappingProvider()
        {
            _viewTypeToMapping = new Dictionary<Type, List<IViewMappingItem>>(MemberInfoComparer.Instance);
            _viewModelToMapping = new Dictionary<Type, Dictionary<string, IViewMappingItem>>(MemberInfoComparer.Instance);
        }

        #endregion

        #region Properties

        public IEnumerable<IViewMappingItem> Mappings
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

        public bool TryGetMappingsByView(Type viewType, out IReadOnlyCollection<IViewMappingItem>? mappings)
        {
            Should.NotBeNull(viewType, nameof(viewType));
            EnsureInitialized();
            if (!_viewTypeToMapping.TryGetValue(viewType, out var item))
            {
                mappings = null;
                return false;
            }

            mappings = item;
            return true;
        }

        public bool TryGetMappingsByViewModel(Type viewModelType, out IReadOnlyCollection<IViewMappingItem>? mappings)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            EnsureInitialized();
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
            {
                mappings = null;
                return false;
            }

            mappings = value.Values.ToReadOnlyCollection();
            return true;
        }

        public bool TryGetMappingByViewModel(Type viewModelType, string? viewName, out IViewMappingItem? mapping)
        {
            Should.NotBeNull(viewModelType, nameof(viewModelType));
            EnsureInitialized();
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
            {
                mapping = null;
                return false;
            }

            viewName = viewName ?? string.Empty;
            if (!value.TryGetValue(viewName, out mapping))
            {
                if (viewName != string.Empty)
                    value.TryGetValue(string.Empty, out mapping);
            }

            return mapping != null;
        }

        #endregion

        #region Methods

        public void AddMapping(IViewMappingItem mappingItem, bool throwOnError = true, bool rewrite = false)
        {
            if (!_viewTypeToMapping.TryGetValue(mappingItem.ViewType, out var list))
            {
                list = new List<IViewMappingItem>();
                _viewTypeToMapping[mappingItem.ViewType] = list;
            }

            list.Add(mappingItem);

            if (!_viewModelToMapping.TryGetValue(mappingItem.ViewModelType, out var value))
            {
                value = new Dictionary<string, IViewMappingItem>();
                _viewModelToMapping[mappingItem.ViewModelType] = value;
            }

            var name = mappingItem.Name ?? string.Empty;
            if (value.TryGetValue(name, out var item))
            {
                if (throwOnError)
                    throw ExceptionManager.DuplicateViewMapping(item.ViewType, item.ViewModelType, item.Name);
                if (!rewrite)
                    return;
            }

            value[name] = mappingItem;
        }

        protected virtual void EnsureInitialized()
        {
        }

        #endregion
    }
}