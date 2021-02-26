using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        private static Action<IReadOnlyMetadataContext, object, object>? _notNullValidateAction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder)
            where T : class =>
            builder.WithValidation(_notNullValidateAction ??= (ctx, k, value) => Should.NotBeNull(value, nameof(value)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyMetadataContext ToContext<T>(this IMetadataContextKey<T> key, T value) => new SingleValueMetadataContext(key.ToValue(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyValuePair<IMetadataContextKey, object?> ToValue<T>(this IMetadataContextKey<T> key, T value)
        {
            Should.NotBeNull(key, nameof(key));
            return new KeyValuePair<IMetadataContextKey, object?>(key, key.SetValue(EmptyMetadataContext.Instance, null, value));
        }

        public static IMetadataContext EnsureInitialized(ref IReadOnlyMetadataContext? metadata)
        {
            if (metadata is IMetadataContext m)
                return m;

            var context = metadata;
            Interlocked.CompareExchange(ref metadata, new MetadataContext(context), context);
            return (IMetadataContext) metadata!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this IReadOnlyMetadataContext? metadata) => metadata == null || metadata.Count == 0;

        public static IMetadataContext ToNonReadonly(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata is IMetadataContext m)
                return m;
            return new MetadataContext(metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyMetadataContext DefaultIfNull(this IReadOnlyMetadataContext? metadata) => metadata ?? EmptyMetadataContext.Instance;

        public static IReadOnlyMetadataContext WithValue<T>(this IReadOnlyMetadataContext? metadata, IMetadataContextKey<T> key, T value)
        {
            if (metadata.IsNullOrEmpty())
                return key.ToContext(value);

            var ctx = metadata.ToNonReadonly();
            ctx.Set(key, value);
            return ctx;
        }

        public static bool TryGet<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> contextKey,
            [MaybeNullWhen(false)] [NotNullIfNotNull("defaultValue")]
            out T value, T? defaultValue = default)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            if (metadataContext.TryGetRaw(contextKey, out var rawValue))
            {
                value = contextKey.GetValue(metadataContext, rawValue);
                return true;
            }

            value = contextKey.GetDefaultValue(metadataContext, defaultValue);
            return false;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static T? Get<T>(this IReadOnlyMetadataContext metadataContext, IReadOnlyMetadataContextKey<T> key, T? defaultValue = default)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.TryGet(key, out var value, defaultValue);
            return value;
        }

        public static string Dump(this IReadOnlyMetadataContext? metadata, string nullResult = "null")
        {
            if (metadata == null)
                return nullResult;
            var builder = new StringBuilder("(");
            foreach (var item in metadata.GetValues())
                builder.Append(item.Key).Append("=").Append(item.Value).Append(";");

            if (builder.Length != 0)
                builder.Remove(builder.Length - 1, 1);
            builder.Append(")");
            return builder.ToString();
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, IMetadataContextKey<T>, T> valueFactory,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, (valueFactory, updateValueFactory), (context, key, s) => s.valueFactory(context, key),
                (context, key, old, s) => s.updateValueFactory(context, key, old));
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T addValue,
            Func<IMetadataContext, IMetadataContextKey<T>, object?, T> updateValueFactory)
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

        public static void Merge(this IMetadataContext metadataContext, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            Should.NotBeNull(metadata, nameof(metadata));
            metadataContext.Merge(metadata.GetValues());
        }

        public static void Merge(this IMetadataContext metadataContext, IEnumerable<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            metadataContext.Merge(ItemOrIEnumerable.FromList(values));
        }

        public static bool Remove(this IMetadataContext metadataContext, IMetadataContextKey contextKey)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.Remove(contextKey, out _);
        }

        public static IReadOnlyMetadataContext GetMetadataOrDefault(this IMetadataOwner<IReadOnlyMetadataContext>? owner, IReadOnlyMetadataContext? defaultValue = null)
        {
            if (owner != null && owner.HasMetadata)
                return owner.Metadata;
            return defaultValue.DefaultIfNull();
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

        internal static T? GetOrDefault<T>(this IMetadataOwner<IReadOnlyMetadataContext> owner, IReadOnlyMetadataContextKey<T> key, T? defaultValue = default)
        {
            if (owner.HasMetadata && owner.Metadata.TryGet(key, out var v, defaultValue))
                return v;
            return key.GetDefaultValue(EmptyMetadataContext.Instance, defaultValue);
        }
    }
}