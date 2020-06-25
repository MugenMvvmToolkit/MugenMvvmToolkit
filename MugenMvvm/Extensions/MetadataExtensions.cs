using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static Action<IReadOnlyMetadataContext, object, object>? _notNullValidateAction;

        #endregion

        #region Methods

        public static bool IsNullOrEmpty(this IReadOnlyMetadataContext? metadata)
        {
            return metadata == null || metadata.Count == 0;
        }

        public static IReadOnlyMetadataContext GetMetadataOrDefault(this IMetadataOwner<IReadOnlyMetadataContext>? owner, IReadOnlyMetadataContext? defaultValue = null)
        {
            if (owner != null && owner.HasMetadata)
                return owner.Metadata;
            return defaultValue ?? Default.Metadata;
        }

        public static IReadOnlyMetadataContext GetReadOnlyMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            var result = metadataContextProvider.TryGetReadOnlyMetadataContext(target, values);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMetadataContextProviderComponent>(metadataContextProvider);
            return result;
        }

        public static IMetadataContext GetMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target = null, ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>> values = default)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            var result = metadataContextProvider.TryGetMetadataContext(target, values);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IMetadataContextProviderComponent>(metadataContextProvider);
            return result;
        }

        public static IMetadataContext LazyInitializeNonReadonly(this IMetadataContextProvider? metadataContextProvider, [NotNull] ref IReadOnlyMetadataContext? metadataContext, object? target)
        {
            if (metadataContext is IMetadataContext m)
                return m;
            metadataContext = metadataContext.ToNonReadonly(target, metadataContextProvider);
            return (IMetadataContext)metadataContext;
        }

        public static bool LazyInitialize(this IMetadataContextProvider? metadataContextProvider, [NotNull] ref IMetadataContext? metadataContext,
            object? target, IReadOnlyCollection<MetadataContextValue>? values = null)
        {
            return metadataContext == null && LazyInitialize(ref metadataContext, metadataContextProvider
                       .DefaultIfNull()
                       .GetMetadataContext(target, new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values)));
        }

        public static IMetadataContext ToNonReadonly(this IReadOnlyMetadataContext? metadata, object? target = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            if (metadata is IMetadataContext m)
                return m;
            return metadataContextProvider.DefaultIfNull().GetMetadataContext(target, new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(metadata));
        }

        public static IReadOnlyMetadataContext GetReadOnlyMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target, IReadOnlyCollection<MetadataContextValue>? values)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            return metadataContextProvider.GetReadOnlyMetadataContext(target, new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values));
        }

        public static IMetadataContext GetMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target, IReadOnlyCollection<MetadataContextValue>? values)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            return metadataContextProvider.GetMetadataContext(target, new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values));
        }

        public static IReadOnlyMetadataContext DefaultIfNull(this IReadOnlyMetadataContext? metadata)
        {
            return metadata ?? Default.Metadata;
        }

        public static string Dump(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
                return string.Empty;
            var builder = new StringBuilder("(");
            var values = metadata.ToArray();
            for (var index = 0; index < values.Length; index++)
            {
                var item = values[index];
                builder.Append(item.ContextKey).Append("=").Append(item.Value).Append(";");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public static void AddHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T, T> key, T handler)
            where T : Delegate?
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (item, value, currentValue, _) => (T)Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T, T> key, T handler)
            where T : Delegate?
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (item, value, currentValue, _) => (T)Delegate.Remove(currentValue, value));
        }

        public static bool TryGet<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] out T value)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.TryGet(contextKey, out value, default);
        }

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> key)
        {
            return metadataContext.Get(key, default);
        }

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> key, [AllowNull]T defaultValue)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static MetadataContextKey.Builder<TGet, TSet> NotNull<TGet, TSet>(this MetadataContextKey.Builder<TGet, TSet> builder)
            where TSet : class
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = (ctx, k, value) => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction);
        }

        public static void ClearMetadata<T>(this IMetadataOwner<T> metadataOwner, bool clearComponents) where T : class, IMetadataContext
        {
            Should.NotBeNull(metadataOwner, nameof(metadataOwner));
            if (metadataOwner.HasMetadata)
            {
                metadataOwner.Metadata.Clear();
                if (clearComponents)
                    metadataOwner.Metadata.ClearComponents(null);
            }
        }

        public static TGet GetOrAdd<TGet, TSet>(this IMetadataContext metadataContext, IMetadataContextKey<TGet, TSet> contextKey, Func<IMetadataContext, TSet> valueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.GetOrAdd(contextKey, valueFactory, (ctx, s) => s(ctx));
        }

        public static void Set<TGet, TSet>(this IMetadataContext metadataContext, IMetadataContextKey<TGet, TSet> contextKey, TSet value)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.Set(contextKey, value, out _);
        }

        public static bool Clear(this IMetadataContext metadataContext, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.Clear(contextKey, out _);
        }

        #endregion
    }
}