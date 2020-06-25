using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class MetadataComponentExtensions
    {
        #region Methods

        public static void OnAdded(this IMetadataContextListener[] listeners, IMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(metadataContext, key, newValue);
        }

        public static void OnChanged(this IMetadataContextListener[] listeners, IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnChanged(metadataContext, key, oldValue, newValue);
        }

        public static void OnRemoved(this IMetadataContextListener[] listeners, IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(metadataContext, key, oldValue);
        }

        public static int GetCount(this IMetadataContextValueManagerComponent[] components)
        {
            Should.NotBeNull(components, nameof(components));
            int count = 0;
            for (int i = 0; i < components.Length; i++)
                count += components[i].GetCount();
            return count;
        }

        public static bool Contains(this IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Contains(contextKey))
                    return true;
            }

            return false;
        }

        public static bool TryGetValue(this IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, out object? rawValue)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetValue(contextKey, out rawValue))
                    return true;
            }

            rawValue = null;
            return false;
        }

        public static bool TrySetValue(this IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey, object? rawValue)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TrySetValue(contextKey, rawValue))
                    return true;
            }
            return false;
        }

        public static bool TryClear(this IMetadataContextValueManagerComponent[] components, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(contextKey, nameof(contextKey));
            bool clear = false;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryClear(contextKey))
                    clear = true;
            }

            return clear;
        }

        public static void Clear(this IMetadataContextValueManagerComponent[] components)
        {
            Should.NotBeNull(components, nameof(components));
            for (int i = 0; i < components.Length; i++)
                components[i].Clear();
        }

        public static IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(this IMetadataContextProviderComponent[] components, object? target,
            ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetReadOnlyMetadataContext(target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IMetadataContext? TryGetMetadataContext(this IMetadataContextProviderComponent[] components, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetMetadataContext(target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnReadOnlyContextCreated(this IMetadataContextProviderListener[] listeners, IMetadataContextProvider metadataContextProvider, IReadOnlyMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReadOnlyContextCreated(metadataContextProvider, metadataContext, target);
        }

        public static void OnContextCreated(this IMetadataContextProviderListener[] listeners, IMetadataContextProvider metadataContextProvider, IMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnContextCreated(metadataContextProvider, metadataContext, target);
        }

        #endregion
    }
}