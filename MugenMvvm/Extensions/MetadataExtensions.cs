using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
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

        public static IReadOnlyMetadataContext ToContext<TGet, TSet>(this IMetadataContextKey<TGet, TSet> key, TSet value) => new SingleValueMetadataContext(key.ToValue(value));

        public static KeyValuePair<IMetadataContextKey, object?> ToValue<TGet, TSet>(this IMetadataContextKey<TGet, TSet> key, TSet value)
        {
            Should.NotBeNull(key, nameof(key));
            return new KeyValuePair<IMetadataContextKey, object?>(key, key.SetValue(Default.Metadata, null, value));
        }

        public static bool IsNullOrEmpty(this IReadOnlyMetadataContext? metadata) => metadata == null || metadata.Count == 0;

        public static IReadOnlyMetadataContext GetMetadataOrDefault(this IMetadataOwner<IReadOnlyMetadataContext>? owner, IReadOnlyMetadataContext? defaultValue = null)
        {
            if (owner != null && owner.HasMetadata)
                return owner.Metadata;
            return defaultValue ?? Default.Metadata;
        }

        public static IMetadataContext ToNonReadonly(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata is IMetadataContext m)
                return m;
            return new MetadataContext(metadata);
        }

        public static IReadOnlyMetadataContext DefaultIfNull(this IReadOnlyMetadataContext? metadata) => metadata ?? Default.Metadata;

        public static string Dump(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
                return string.Empty;
            var builder = new StringBuilder("(");
            var values = metadata.ToArray();
            for (var index = 0; index < values.Length; index++)
            {
                var item = values[index];
                builder.Append(item.Key).Append("=").Append(item.Value).Append(";");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public static void AddHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T, T> key, T handler)
            where T : Delegate?
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?) null, (item, value, currentValue, _) => (T) Delegate.Combine(currentValue, value)!);
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T, T> key, T handler)
            where T : Delegate?
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?) null, (item, value, currentValue, _) => (T) Delegate.Remove(currentValue, value)!);
        }

        public static bool TryGet<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] out T value)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.TryGet(contextKey, out value, default);
        }

        [return: MaybeNull]
        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> key) => metadataContext.Get(key, default);

        [return: MaybeNull]
        [return: NotNullIfNotNull("defaultValue")]
        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> key, [AllowNull] T defaultValue)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static bool TryGet<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] [NotNullIfNotNull("defaultValue")]
            out T value, [AllowNull] T defaultValue)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            if (metadataContext.TryGetRaw(contextKey, out var obj))
            {
                value = contextKey.GetValue(metadataContext, obj);
                return true;
            }

            value = contextKey.GetDefaultValue(metadataContext, defaultValue);
            return false;
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
                    metadataOwner.Metadata.ClearComponents();
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

        public static bool Remove(this IMetadataContext metadataContext, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.Remove(contextKey, out _);
        }

        internal static IMetadataContext LazyInitialize(ref IReadOnlyMetadataContext? metadata)
        {
            if (metadata is IMetadataContext m)
                return m;

            var context = metadata;
            Interlocked.CompareExchange(ref metadata, new MetadataContext(metadata), context);
            return (IMetadataContext) metadata!;
        }

        #endregion
    }
}