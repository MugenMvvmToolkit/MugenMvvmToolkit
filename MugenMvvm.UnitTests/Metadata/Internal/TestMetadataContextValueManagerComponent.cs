﻿using System;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Metadata.Internal
{
    public class TestMetadataContextValueManagerComponent : IMetadataContextValueManagerComponent, IHasPriority
    {
        private readonly IMetadataContext _context;

        public TestMetadataContextValueManagerComponent(IMetadataContext context)
        {
            _context = context;
        }

        public Func<int>? GetCount { get; set; }

        public Func<IEnumerable<KeyValuePair<IMetadataContextKey, object?>>>? GetValues { get; set; }

        public Func<IMetadataContextKey, bool>? Contains { get; set; }

        public Func<IMetadataContextKey, MetadataOperationType, (bool, object?)>? TryGetValue { get; set; }

        public Func<IMetadataContextKey, object?, bool>? TrySetValue { get; set; }

        public Func<IMetadataContextKey, bool>? TryClear { get; set; }

        public Action? Clear { get; set; }

        public int Priority { get; set; }

        int IMetadataContextValueManagerComponent.GetCount(IMetadataContext context)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return GetCount?.Invoke() ?? 0;
        }

        void IMetadataContextValueManagerComponent.GetValues(IMetadataContext context, MetadataOperationType operationType,
            ref ItemOrListEditor<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            var array = GetValues?.Invoke() ?? Default.Array<KeyValuePair<IMetadataContextKey, object?>>();
            values.AddRange(array);
        }

        bool IMetadataContextValueManagerComponent.Contains(IMetadataContext context, IMetadataContextKey contextKey)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return Contains?.Invoke(contextKey) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryGetValue(IMetadataContext context, IMetadataContextKey contextKey, MetadataOperationType operationType, out object? rawValue)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            var tuple = TryGetValue?.Invoke(contextKey, operationType);
            if (tuple == null)
            {
                rawValue = null;
                return false;
            }

            rawValue = tuple.Value.Item2;
            return tuple.Value.Item1;
        }

        bool IMetadataContextValueManagerComponent.TrySetValue(IMetadataContext context, IMetadataContextKey contextKey, object? rawValue)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            return TrySetValue?.Invoke(contextKey, rawValue) ?? false;
        }

        bool IMetadataContextValueManagerComponent.TryRemove(IMetadataContext context, IMetadataContextKey contextKey)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            context.ShouldEqual(_context);
            return TryClear?.Invoke(contextKey) ?? false;
        }

        void IMetadataContextValueManagerComponent.Clear(IMetadataContext context)
        {
            ReferenceEquals(_context, context).ShouldBeTrue();
            context.ShouldEqual(_context);
            Clear?.Invoke();
        }
    }
}