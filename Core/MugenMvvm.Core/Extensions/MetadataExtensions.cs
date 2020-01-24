using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Metadata;
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

        public static void Aggregate(this IReadOnlyMetadataContext? metadata, ref IReadOnlyMetadataContext? currentMetadata)
        {
            if (metadata.IsNullOrEmpty())
                return;

            if (currentMetadata == null)
                currentMetadata = metadata;
            else
            {
                if (!(currentMetadata is AggregatedMetadataContext aggregatedMetadata))
                {
                    aggregatedMetadata = new AggregatedMetadataContext(currentMetadata);
                    currentMetadata = aggregatedMetadata;
                }

                aggregatedMetadata.Aggregate(metadata!);
            }
        }

        public static IMetadataContext LazyInitializeNonReadonly(this IMetadataContextProvider? metadataContextProvider, [NotNull] ref IReadOnlyMetadataContext? metadataContext, object? target)
        {
            if (metadataContext is IMetadataContext m)
                return m;
            LazyInitialize(ref metadataContext, metadataContext.ToNonReadonly(target, metadataContextProvider));
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

        public static IReadOnlyMetadataContext GetReadOnlyMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target = null, IReadOnlyCollection<MetadataContextValue>? values = null)
        {
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            return metadataContextProvider.GetReadOnlyMetadataContext(target, new ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>(values));
        }

        public static IMetadataContext GetMetadataContext(this IMetadataContextProvider metadataContextProvider, object? target = null, IReadOnlyCollection<MetadataContextValue>? values = null)
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

        public static void AddHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (item, value, currentValue, _) => (T)Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (item, value, currentValue, _) => (T)Delegate.Remove(currentValue, value));
        }

        [return: MaybeNull, NotNullIfNotNull("defaultValue")]
        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IMetadataContextKey<T> key, T defaultValue = default!)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder) where T : class?
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = (ctx, k, value) => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction!);
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

        public static T GetOrAdd<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, T> valueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.GetOrAdd(contextKey, valueFactory, (ctx, s) => s(ctx));
        }

        #endregion
    }
}