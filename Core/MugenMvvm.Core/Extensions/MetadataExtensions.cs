using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static Action<IReadOnlyMetadataContext, object, object>? _notNullValidateAction;

        #endregion

        #region Methods

        public static IReadOnlyMetadataContext GetMetadataOrDefault(this IMetadataOwner<IReadOnlyMetadataContext>? owner, IReadOnlyMetadataContext? defaultValue = null)
        {
            if (owner != null && owner.HasMetadata)
                return owner.Metadata;
            return defaultValue ?? Default.Metadata;
        }

        public static bool LazyInitialize(this IMetadataContextProvider? provider, [EnsuresNotNull] ref IMetadataContext? metadataContext,
            object? target, IEnumerable<MetadataContextValue>? values = null)
        {
            return metadataContext == null && LazyInitialize(ref metadataContext, GetMetadataContext(target, values, provider));
        }

        public static IMetadataContext ToNonReadonly(this IReadOnlyMetadataContext? metadata, object? target = null, IMetadataContextProvider? contextProvider = null)
        {
            if (metadata is IMetadataContext m)
                return m;
            return contextProvider.ServiceIfNull().GetMetadataContext(target, metadata);
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
            metadata.AddOrUpdate(key, handler, (object?)null, (object?)null, (item, value, currentValue, state1, state2) => (T)Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?)null, (object?)null, (item, value, currentValue, state1, state2) => (T)Delegate.Remove(currentValue, value));
        }

        public static T Get<T>(this IReadOnlyMetadataContext metadataContext, IMetadataContextKey<T> key, T defaultValue = default)
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

        public static IReadOnlyMetadataContext ToReadOnlyMetadataContext(this IEnumerable<MetadataContextValue>? values, object? target = null,
            IMetadataContextProvider? provider = null)
        {
            return GetReadOnlyMetadataContext(target, values, provider);
        }

        public static IMetadataContext ToMetadataContext(this IEnumerable<MetadataContextValue>? values, object? target = null, IMetadataContextProvider? provider = null)
        {
            return GetMetadataContext(target, values, provider);
        }

        public static IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue>? values = null,
            IMetadataContextProvider? provider = null)
        {
            return provider.ServiceIfNull().GetReadOnlyMetadataContext(target, values);
        }

        public static IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue>? values = null, IMetadataContextProvider? provider = null)
        {
            return provider.ServiceIfNull().GetMetadataContext(target, values);
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

        public static T GetOrAdd<T, TState1>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, TState1 state1,
            Func<IMetadataContext, TState1, T> valueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.GetOrAdd(contextKey, state1, valueFactory, (ctx, s1, s2) => s2(ctx, s1));
        }

        public static T GetOrAdd<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, T> valueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.GetOrAdd(contextKey, valueFactory, valueFactory, (ctx, s1, _) => s1(ctx));
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, Func<IMetadataContext, T> valueFactory,
            UpdateValueDelegate<IMetadataContext, Func<IMetadataContext, T>, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, valueFactory, updateValueFactory, (ctx, s1, _) => s1(ctx), (ctx, _, cV, s1, s2) => s2(ctx, s1, cV));
        }

        public static T AddOrUpdate<T, TState1>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T addValue, TState1 state1,
            UpdateValueDelegate<IMetadataContext, T, T, TState1> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, addValue, state1, updateValueFactory, (ctx, v, cV, s1, s2) => s2(ctx, v, cV, s1));
        }

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T addValue,
            UpdateValueDelegate<IMetadataContext, T, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, addValue, updateValueFactory, updateValueFactory, (ctx, v, cV, s1, _) => s1(ctx, v, cV));
        }

        #endregion
    }
}