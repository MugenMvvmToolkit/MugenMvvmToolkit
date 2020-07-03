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

        public static void OnAdded(this IMetadataContextListener[] listeners, IMetadataContext context, IMetadataContextKey key, object? newValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(context, key, newValue);
        }

        public static void OnChanged(this IMetadataContextListener[] listeners, IMetadataContext context, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnChanged(context, key, oldValue, newValue);
        }

        public static void OnRemoved(this IMetadataContextListener[] listeners, IMetadataContext context, IMetadataContextKey key, object? oldValue)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(context, key, oldValue);
        }

        public static int GetCount(this IMetadataContextValueManagerComponent[] components, IMetadataContext context)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            int count = 0;
            for (int i = 0; i < components.Length; i++)
                count += components[i].GetCount(context);
            return count;
        }

        public static bool Contains(this IMetadataContextValueManagerComponent[] components, IMetadataContext context, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Contains(context, contextKey))
                    return true;
            }

            return false;
        }

        public static bool TryGetValue(this IMetadataContextValueManagerComponent[] components, IMetadataContext context, IMetadataContextKey contextKey, out object? rawValue)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetValue(context, contextKey, out rawValue))
                    return true;
            }

            rawValue = null;
            return false;
        }

        public static bool TrySetValue(this IMetadataContextValueManagerComponent[] components, IMetadataContext context, IMetadataContextKey contextKey, object? rawValue)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TrySetValue(context, contextKey, rawValue))
                    return true;
            }
            return false;
        }

        public static bool TryClear(this IMetadataContextValueManagerComponent[] components, IMetadataContext context, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            bool clear = false;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].TryClear(context, contextKey))
                    clear = true;
            }

            return clear;
        }

        public static void Clear(this IMetadataContextValueManagerComponent[] components, IMetadataContext context)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            for (int i = 0; i < components.Length; i++)
                components[i].Clear(context);
        }

        public static IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(this IMetadataContextProviderComponent[] components, IMetadataContextManager metadataContextManager, object? target,
            ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(metadataContextManager, nameof(metadataContextManager));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetReadOnlyMetadataContext(metadataContextManager, target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static IMetadataContext? TryGetMetadataContext(this IMetadataContextProviderComponent[] components, IMetadataContextManager metadataContextManager, object? target, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(metadataContextManager, nameof(metadataContextManager));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetMetadataContext(metadataContextManager, target, values);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnReadOnlyContextCreated(this IMetadataContextManagerListener[] listeners, IMetadataContextManager metadataContextManager, IReadOnlyMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContextManager, nameof(metadataContextManager));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReadOnlyContextCreated(metadataContextManager, metadataContext, target);
        }

        public static void OnContextCreated(this IMetadataContextManagerListener[] listeners, IMetadataContextManager metadataContextManager, IMetadataContext metadataContext, object? target)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(metadataContextManager, nameof(metadataContextManager));
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnContextCreated(metadataContextManager, metadataContext, target);
        }

        #endregion
    }
}