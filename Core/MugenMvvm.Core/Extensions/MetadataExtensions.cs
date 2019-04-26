using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

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
            metadata.AddOrUpdate(key, handler, (object?) null, (object?) null, (item, value, currentValue, state1, state2) => (T) Delegate.Combine(currentValue, value));
        }

        public static void RemoveHandler<T>(this IMetadataContext metadata, IMetadataContextKey<T> key, T handler)
            where T : Delegate
        {
            Should.NotBeNull(metadata, nameof(metadata));
            metadata.AddOrUpdate(key, handler, (object?) null, (object?) null, (item, value, currentValue, state1, state2) => (T) Delegate.Remove(currentValue, value));
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

        #endregion
    }
}