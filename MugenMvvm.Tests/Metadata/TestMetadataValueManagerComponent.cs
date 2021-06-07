using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Metadata
{
    public class TestMetadataValueManagerComponent : IMetadataValueManagerComponent, IHasPriority
    {
        public Func<IMetadataContext, int>? GetCount { get; set; }

        public Func<IMetadataContext, IEnumerable<KeyValuePair<IMetadataContextKey, object?>>>? GetValues { get; set; }

        public Func<IMetadataContext, IMetadataContextKey, bool>? Contains { get; set; }

        public Func<IMetadataContext, IMetadataContextKey, MetadataOperationType, (bool, object?)>? TryGetValue { get; set; }

        public Func<IMetadataContext, IMetadataContextKey, object?, bool>? TrySetValue { get; set; }

        public Func<IMetadataContext, IMetadataContextKey, bool>? TryClear { get; set; }

        public Action<IMetadataContext>? Clear { get; set; }

        public int Priority { get; set; }

        int IMetadataValueManagerComponent.GetCount(IMetadataContext context) => GetCount?.Invoke(context) ?? 0;

        void IMetadataValueManagerComponent.GetValues(IMetadataContext context, MetadataOperationType operationType,
            ref ItemOrListEditor<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            var array = GetValues?.Invoke(context) ?? Array.Empty<KeyValuePair<IMetadataContextKey, object?>>();
            values.AddRange(array);
        }

        bool IMetadataValueManagerComponent.Contains(IMetadataContext context, IMetadataContextKey contextKey) => Contains?.Invoke(context, contextKey) ?? false;

        bool IMetadataValueManagerComponent.TryGetValue(IMetadataContext context, IMetadataContextKey contextKey, MetadataOperationType operationType, out object? rawValue)
        {
            var tuple = TryGetValue?.Invoke(context, contextKey, operationType);
            if (tuple == null)
            {
                rawValue = null;
                return false;
            }

            rawValue = tuple.Value.Item2;
            return tuple.Value.Item1;
        }

        bool IMetadataValueManagerComponent.TrySetValue(IMetadataContext context, IMetadataContextKey contextKey, object? rawValue) =>
            TrySetValue?.Invoke(context, contextKey, rawValue) ?? false;

        bool IMetadataValueManagerComponent.TryRemove(IMetadataContext context, IMetadataContextKey contextKey) => TryClear?.Invoke(context, contextKey) ?? false;

        void IMetadataValueManagerComponent.Clear(IMetadataContext context) => Clear?.Invoke(context);
    }
}