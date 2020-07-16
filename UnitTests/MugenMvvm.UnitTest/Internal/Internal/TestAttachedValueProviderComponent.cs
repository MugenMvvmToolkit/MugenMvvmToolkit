﻿using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestAttachedValueProviderComponent : IAttachedValueProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<object?, Type, object?, Type, Func<object, KeyValuePair<string, object?>, object, bool>?, ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>>>? TryGetValues
        {
            get;
            set;
        }

        public Func<object, string, object?>? TryGet { get; set; }

        public Func<object, string, bool>? Contains { get; set; }

        public Func<object?, Type, string, object?, Type, object?, Type, UpdateValueDelegate<object?, object?, object?, object?, object?>, object?>? AddOrUpdate { get; set; }

        public Func<object?, Type, string, object?, Type, Func<object?, object?, object?>, Type, UpdateValueDelegate<object?, Func<object?, object?, object?>, object?, object?, object?>, object?>? AddOrUpdate1
        {
            get;
            set;
        }

        public Func<object, string, object?, Type, object?>? GetOrAdd { get; set; }

        public Func<object?, Type, string, object?, Type, Type, Func<object?, object?, object?>, object?>? GetOrAdd1 { get; set; }

        public SetDelegate? Set { get; set; }

        public ClearDelegate? ClearKey { get; set; }

        public Func<object, bool>? Clear { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachedValueProviderComponent.IsSupported(object item, IReadOnlyMetadataContext? metadata)
        {
            return IsSupported?.Invoke(item, metadata) ?? false;
        }

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> IAttachedValueProviderComponent.TryGetValues<TItem, TState>(TItem item, in TState state,
            Func<TItem, KeyValuePair<string, object?>, TState, bool>? predicate)
        {
            return TryGetValues!.Invoke(item, typeof(TItem), state, typeof(TState),
                predicate == null ? (Func<object, KeyValuePair<string, object?>, object, bool>?)null : (o, pair, arg3) => predicate!((TItem)o, pair, (TState)arg3));
        }

        bool IAttachedValueProviderComponent.TryGet(object item, string path, out object? value)
        {
            var result = TryGet!.Invoke(item, path);
            if (result == null)
            {
                value = default!;
                return false;
            }

            value = result;
            return true;
        }

        bool IAttachedValueProviderComponent.Contains(object item, string path)
        {
            return Contains!.Invoke(item, path);
        }

        TValue IAttachedValueProviderComponent.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, TValue addValue, in TState state, UpdateValueDelegate<TItem, TValue, TValue, TState, TValue> updateValueFactory)
        {
            return (TValue)AddOrUpdate!.Invoke(item, typeof(TItem), path, addValue, typeof(TValue), state, typeof(TState),
                (o, value, currentValue, state1) => updateValueFactory((TItem)o!, (TValue)value!, (TValue)currentValue!, (TState)state1!))!;
        }

        TValue IAttachedValueProviderComponent.AddOrUpdate<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> addValueFactory,
            UpdateValueDelegate<TItem, TValue, TState, TValue> updateValueFactory)
        {
            Func<object?, object?, object?> func = (o, o1) => addValueFactory((TItem)o!, (TState)o1!);
            return (TValue)AddOrUpdate1!.Invoke(item, typeof(TItem), path, state, typeof(TState), func, typeof(TValue),
                (o, value, currentValue, state1) => { return updateValueFactory((TItem)o!, addValueFactory, (TValue)currentValue!, (TState)state1!); })!;
        }

        TValue IAttachedValueProviderComponent.GetOrAdd<TValue>(object item, string path, TValue value)
        {
            return (TValue)GetOrAdd!.Invoke(item, path, value, typeof(TValue))!;
        }

        TValue IAttachedValueProviderComponent.GetOrAdd<TItem, TValue, TState>(TItem item, string path, in TState state, Func<TItem, TState, TValue> valueFactory)
        {
            return (TValue)GetOrAdd1!.Invoke(item, typeof(TItem), path, state, typeof(TState), typeof(TValue), (o, o1) => valueFactory((TItem)o!, (TState)o1!))!;
        }

        void IAttachedValueProviderComponent.Set(object item, string path, object? value, out object? oldValue)
        {
            Set!.Invoke(item, path, value!, out oldValue);
        }

        bool IAttachedValueProviderComponent.Clear(object item, string path, out object? oldValue)
        {
            return ClearKey!.Invoke(item, path, out oldValue);
        }

        bool IAttachedValueProviderComponent.Clear(object item)
        {
            return Clear!.Invoke(item);
        }

        #endregion

        #region Nested types

        public delegate bool ClearDelegate(object item, string path, out object? oldValue);

        public delegate void SetDelegate(object item, string path, object? value, out object? oldValue);

        #endregion
    }
}