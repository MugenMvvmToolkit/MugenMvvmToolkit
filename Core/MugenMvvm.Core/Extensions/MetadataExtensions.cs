﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MugenMvvm.Delegates;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Fields

        private static Action<IReadOnlyMetadataContext, object, object>? _notNullValidateAction;

        #endregion

        #region Methods

        public static IReadOnlyMetadataContext DefaultIfNull(this IReadOnlyMetadataContext metadata)//todo bug R#
        {
            return metadata ?? Default.MetadataContext;
        }

        public static string Dump(this IReadOnlyMetadataContext? metadata)
        {
            if (metadata == null)
                return string.Empty;
            var builder = new StringBuilder("(");
            var values = metadata.ToArray();
            foreach (var item in values)
                builder.Append(item.ContextKey).Append("=").Append(item.Value).Append(";");
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

        public static MetadataContextKey.Builder<T> NotNull<T>(this MetadataContextKey.Builder<T> builder) where T : class ?
        {
            if (_notNullValidateAction == null)
                _notNullValidateAction = (ctx, k, value) => Should.NotBeNull(value, nameof(value));
            return builder.WithValidation(_notNullValidateAction!);
        }

        public static IReadOnlyMetadataContext ToReadOnlyMetadataContext(this IEnumerable<MetadataContextValue>? values, object target = null,
            IMetadataContextProvider? provider = null)
        {
            return GetReadOnlyMetadataContext(target, values, provider);
        }

        public static IMetadataContext ToMetadataContext(this IEnumerable<MetadataContextValue>? values, object target = null, IMetadataContextProvider? provider = null)
        {
            return GetMetadataContext(target, values, provider);
        }

        public static IObservableMetadataContext ToObservableMetadataContext(this IEnumerable<MetadataContextValue> values, object? target = null,
            IMetadataContextProvider? provider = null)
        {
            return GetObservableMetadataContext(target, values, provider);
        }

        public static IReadOnlyMetadataContext GetReadOnlyMetadataContext(object? target, IEnumerable<MetadataContextValue> values = null,
            IMetadataContextProvider? provider = null)
        {
            return provider.ServiceIfNull().GetReadOnlyMetadataContext(target, values);
        }

        public static IMetadataContext GetMetadataContext(object? target, IEnumerable<MetadataContextValue> values = null, IMetadataContextProvider? provider = null)
        {
            return provider.ServiceIfNull().GetMetadataContext(target, values);
        }

        public static IObservableMetadataContext GetObservableMetadataContext(object? target, IEnumerable<MetadataContextValue> values = null,
            IMetadataContextProvider? provider = null)
        {
            return provider.ServiceIfNull().GetObservableMetadataContext(target, values);
        }

        public static void ClearMetadata<T>(this IHasMetadata<T> hasMetadata) where T : class, IMetadataContext
        {
            Should.NotBeNull(hasMetadata, nameof(hasMetadata));
            if (hasMetadata.IsMetadataInitialized)
            {
                hasMetadata.Metadata.Clear();
            }
        }

        public static void ClearMetadata<T>(this IHasMetadata<T> hasMetadata, bool clearListeners)
            where T : class, IObservableMetadataContext
        {
            Should.NotBeNull(hasMetadata, nameof(hasMetadata));
            if (hasMetadata.IsMetadataInitialized)
            {
                hasMetadata.Metadata.Clear();
                if (clearListeners)
                    hasMetadata.Metadata.RemoveAllListeners();
            }
        }

        public static T GetOrAdd<T, TState1>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, TState1 state1, Func<IMetadataContext, TState1, T> valueFactory)
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

        public static T AddOrUpdate<T>(this IMetadataContext metadataContext, IMetadataContextKey<T> contextKey, T addValue, UpdateValueDelegate<IMetadataContext, T, T> updateValueFactory)
        {
            Should.NotBeNull(metadataContext, nameof(metadataContext));
            return metadataContext.AddOrUpdate(contextKey, addValue, updateValueFactory, updateValueFactory, (ctx, v, cV, s1, _) => s1(ctx, v, cV));
        }

        #endregion
    }
}