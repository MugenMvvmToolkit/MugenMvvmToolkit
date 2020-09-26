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

        public static IReadOnlyMetadataContext ToContext<T>(this IMetadataContextKey<T> key, T value) => new SingleValueMetadataContext(key.ToValue(value));

        public static KeyValuePair<IMetadataContextKey, object?> ToValue<T>(this IMetadataContextKey<T> key, T value)
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

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder)
            where T : class =>
            builder.WithValidation(_notNullValidateAction ??= (ctx, k, value) => Should.NotBeNull(value, nameof(value)));

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

        public static bool TryGet<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey, [MaybeNullWhen(false)] out T value)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.TryGet(contextKey, out value, default);
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, IMetadataContextKey<T>, T> valueFactory,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, (valueFactory, updateValueFactory), (context, key, s) => s.valueFactory(context, key), (context, key, old, s) => s.updateValueFactory(context, key, old));
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T addValue, Func<IMetadataContext, IMetadataContextKey<T>, object?, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, addValue, updateValueFactory, (context, key, old, s) => s(context, key, old));
        }

        public static T GetOrAdd<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, IMetadataContextKey<T>, T> valueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.GetOrAdd(contextKey, valueFactory, (ctx, k, s) => s(ctx, k));
        }

        public static void Set<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T value)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.Set(contextKey, value, out _);
        }

        public static bool Remove(this IMetadataContext metadataContext, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.Remove(contextKey, out _);
        }

        public static string Dump(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
                return "null";
            var builder = new StringBuilder("(");
            var values = metadata.ToArray();
            for (var index = 0; index < values.Length; index++)
            {
                var item = values[index];
                builder.Append(item.Key).Append("=").Append(item.Value).Append(";");
            }

            if (builder.Length != 0)
                builder.Remove(builder.Length - 1, 1);
            builder.Append(")");
            return builder.ToString();
        }

        internal static bool TryGetFromRaw<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey, bool hasValue, object? rawValue, out T value, [AllowNull] T defaultValue)
        {
            Should.NotBeNull(contextKey, nameof(contextKey));
            if (hasValue)
            {
                value = contextKey.GetValue(metadataContext, rawValue);
                return true;
            }

            value = contextKey.GetDefaultValue(metadataContext, defaultValue);
            return false;
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