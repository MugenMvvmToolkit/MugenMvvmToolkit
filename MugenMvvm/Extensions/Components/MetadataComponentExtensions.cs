using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class MetadataComponentExtensions
    {
        public static void OnAdded(this ItemOrArray<IMetadataContextListener> listeners, IMetadataContext context, IMetadataContextKey key, object? newValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            foreach (var c in listeners)
                c.OnAdded(context, key, newValue);
        }

        public static void OnChanged(this ItemOrArray<IMetadataContextListener> listeners, IMetadataContext context, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            foreach (var c in listeners)
                c.OnChanged(context, key, oldValue, newValue);
        }

        public static void OnRemoved(this ItemOrArray<IMetadataContextListener> listeners, IMetadataContext context, IMetadataContextKey key, object? oldValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(key, nameof(key));
            foreach (var c in listeners)
                c.OnRemoved(context, key, oldValue);
        }

        public static void GetValues(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context,
            MetadataOperationType operationType, ref ItemOrListEditor<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(operationType, nameof(operationType));
            foreach (var c in components)
                c.GetValues(context, operationType, ref values);
        }

        public static int GetCount(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            var count = 0;
            foreach (var c in components)
                count += c.GetCount(context);

            return count;
        }

        public static bool Contains(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            foreach (var c in components)
            {
                if (c.Contains(context, contextKey))
                    return true;
            }

            return false;
        }

        public static bool TryGetValue(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context, IMetadataContextKey contextKey,
            MetadataOperationType operationType, out object? rawValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            Should.NotBeNull(operationType, nameof(operationType));
            foreach (var c in components)
            {
                if (c.TryGetValue(context, contextKey, operationType, out rawValue))
                    return true;
            }

            rawValue = null;
            return false;
        }

        public static bool TrySetValue(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context, IMetadataContextKey contextKey,
            object? rawValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            foreach (var c in components)
            {
                if (c.TrySetValue(context, contextKey, rawValue))
                    return true;
            }

            return false;
        }

        public static bool TryClear(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(contextKey, nameof(contextKey));
            var clear = false;
            foreach (var c in components)
            {
                if (c.TryRemove(context, contextKey))
                    clear = true;
            }

            return clear;
        }

        public static void Clear(this ItemOrArray<IMetadataValueManagerComponent> components, IMetadataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            foreach (var c in components)
                c.Clear(context);
        }
    }
}