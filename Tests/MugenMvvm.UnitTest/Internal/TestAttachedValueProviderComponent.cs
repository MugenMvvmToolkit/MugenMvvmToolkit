﻿using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal
{
    public class TestAttachedValueProviderComponent : IAttachedValueProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, IReadOnlyMetadataContext?, bool> IsSupported { get; set; }

        public Func<object?, Type, object?, Type, Func<object, KeyValuePair<string, object?>, object, bool>?, IReadOnlyList<KeyValuePair<string, object?>>?>? TryGetValues { get; set; }

        public Func<object, string, Type, object?>? TryGet { get; set; }

        public Func<object, string, bool>? Contains { get; set; }

        public Func<object?, Type, string, object?, Type, object?, Type, UpdateValueDelegate<object?, object?, object?, object?>, object?>? AddOrUpdate { get; set; }

        public Func<object?, Type, string, object?, Type, Func<object?, object?, object?>, Type, UpdateValueDelegate<object?, Func<object?, object?, object?>, object?, object?>, object?>? AddOrUpdate1 { get; set; }

        public Func<object, string, object?, Type, object?>? GetOrAdd { get; set; }

        public Func<object?, Type, string, object?, Type, Type, Func<object?, object?, object?>, object?>? GetOrAdd1 { get; set; }

        public Action<object, string, object?, Type>? Set { get; set; }

        public Func<object, string, bool>? Clear { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachedValueProviderComponent.IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return IsSupported?.Invoke(item, metadata) ?? false;
        }

        IReadOnlyList<KeyValuePair<string, object?>>? IAttachedValueProviderComponent.TryGetValues<TItem, TState>(TItem item, TState state, Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate)
        {
            return TryGetValues.Invoke(item, typeof(TItem), state, typeof(TState),
                predicate == null ? (Func<object, KeyValuePair<string, object?>, object, bool>)null : (o, pair, arg3) => predicate((TItem)o, pair, (TState)arg3));
        }

        bool IAttachedValueProviderComponent.TryGet<TValue>(object item, string path, out TValue value)
        {
            var result = TryGet.Invoke(item, path, typeof(TValue));
            if (result == null)
            {
                value = default;
                return false;
            }

            value = (TValue)result;
            return true;
        }

        bool IAttachedValueProviderComponent.Contains(object item, string path)
        {
            return Contains.Invoke(item, path);
        }

        TValue IAttachedValueProviderComponent.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, TState state, UpdateValueDelegate<TItem, TValue, TValue, TState> updateValueFactory)
        {
            return (TValue)AddOrUpdate.Invoke(item, typeof(TItem), path, addValue, typeof(TValue), state, typeof(TState),
                (o, value, currentValue, state1) => updateValueFactory((TItem)o, (TValue)value, (TValue)currentValue, (TState)state1));
        }

        TValue IAttachedValueProviderComponent.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState, TValue>, TValue, TState> updateValueFactory)
        {
            Func<object?, object?, object?> func = (o, o1) => addValueFactory((TItem)o, (TState)o1);
            return (TValue)AddOrUpdate1.Invoke(item, typeof(TItem), path, state, typeof(TState), func, typeof(TValue),
                (o, value, currentValue, state1) =>
                {
                    return updateValueFactory((TItem)o, addValueFactory, (TValue)currentValue, (TState)state1);
                });
        }

        TValue IAttachedValueProviderComponent.GetOrAdd<TValue>(object item, string path, TValue value)
        {
            return (TValue)GetOrAdd.Invoke(item, path, value, typeof(TValue));
        }

        TValue IAttachedValueProviderComponent.GetOrAdd<TItem, TValue, TState>(TItem item, string path, TState state, Func<TItem, TState, TValue> valueFactory)
        {
            return (TValue)GetOrAdd1.Invoke(item, typeof(TItem), path, state, typeof(TState), typeof(TValue), (o, o1) => valueFactory((TItem)o, (TState)o1));
        }

        void IAttachedValueProviderComponent.Set<TValue>(object item, string path, TValue value)
        {
            Set.Invoke(item, path, value, typeof(TValue));
        }

        bool IAttachedValueProviderComponent.Clear(object item, string? path)
        {
            return Clear.Invoke(item, path);
        }

        #endregion
    }
}