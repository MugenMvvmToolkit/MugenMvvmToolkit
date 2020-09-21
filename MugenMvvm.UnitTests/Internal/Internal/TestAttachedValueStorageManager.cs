using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestAttachedValueStorageManager : IAttachedValueStorageManager, IHasPriority
    {
        #region Properties

        public Func<object, object?, object?, Func<object, KeyValuePair<string, object?>, object?, bool>?, ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>>>? TryGetValues
        {
            get;
            set;
        }

        public Func<object, object?, int>? GetCount { get; set; }

        public Func<object, object?, string, object?>? TryGet { get; set; }

        public Func<object, object?, string, bool>? Contains { get; set; }

        public Func<object, object?, string, object?, object?, UpdateValueDelegate<object, object?, object?, object?, object?>, object?>? AddOrUpdate { get; set; }

        public Func<object, object?, string, object?, Func<object, object?, object?>, UpdateValueDelegate<object, object?, object?, object?>, object?>? AddOrUpdate1 { get; set; }

        public Func<object, object?, string, object?, object?>? GetOrAdd { get; set; }

        public Func<object, object?, string, object?, Func<object, object?, object?>, object?>? GetOrAdd1 { get; set; }

        public SetDelegate? Set { get; set; }

        public ClearDelegate? ClearKey { get; set; }

        public Func<object, bool>? Clear { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        int IAttachedValueStorageManager.GetCount(object item, ref object? internalState) => GetCount!.Invoke(item, internalState);

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> IAttachedValueStorageManager.GetValues<TState>(object item, TState state,
            Func<object, KeyValuePair<string, object?>, TState, bool>? predicate, ref object? internalState) =>
            TryGetValues!.Invoke(item, internalState, state, predicate == null ? null : new Func<object, KeyValuePair<string, object?>, object?, bool>((o, pair, arg3) => predicate(o, pair, (TState)arg3!)));

        bool IAttachedValueStorageManager.TryGet(object item, string path, ref object? internalState, out object? value)
        {
            var result = TryGet!.Invoke(item, internalState, path);
            if (result == null)
            {
                value = default!;
                return false;
            }

            value = result;
            return true;
        }

        bool IAttachedValueStorageManager.Contains(object item, string path, ref object? internalState) => Contains!.Invoke(item, internalState, path);

        TValue IAttachedValueStorageManager.AddOrUpdate<TValue, TState>(object item, string path, TValue addValue, TState state,
            UpdateValueDelegate<object, TValue, TValue, TState, TValue> updateValueFactory, ref object? internalState) =>
            (TValue)AddOrUpdate!.Invoke(item, internalState, path, addValue, state, (o, value, currentValue, state1) => updateValueFactory(o, (TValue)value!, (TValue)currentValue!, (TState)state1!))!;

        TValue IAttachedValueStorageManager.AddOrUpdate<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> addValueFactory,
            UpdateValueDelegate<object, TValue, TState, TValue> updateValueFactory, ref object? internalState) =>
            (TValue)AddOrUpdate1!.Invoke(item, internalState, path, state, (o, o1) => addValueFactory(o, (TState)o1!),
                (o, factory, value, state1) => updateValueFactory(o, (o1, state2) => (TValue)factory(o1, state2)!, (TValue)value!, (TState)state1!))!;

        TValue IAttachedValueStorageManager.GetOrAdd<TValue>(object item, string path, TValue value, ref object? internalState) => (TValue)GetOrAdd!.Invoke(item, internalState, path, value)!;

        TValue IAttachedValueStorageManager.GetOrAdd<TValue, TState>(object item, string path, TState state, Func<object, TState, TValue> valueFactory, ref object? internalState) =>
            (TValue)GetOrAdd1!.Invoke(item, internalState, path, state, (o, o1) => valueFactory(o, (TState)o1!))!;

        void IAttachedValueStorageManager.Set(object item, string path, object? value, ref object? internalState, out object? oldValue) => Set!.Invoke(item, internalState, path, value!, out oldValue);

        bool IAttachedValueStorageManager.Remove(object item, string path, ref object? internalState, out object? oldValue) => ClearKey!.Invoke(item, internalState, path, out oldValue);

        bool IAttachedValueStorageManager.Clear(object item, ref object? internalState) => Clear!.Invoke(item);

        #endregion

        #region Nested types

        public delegate bool ClearDelegate(object item, object? internalState, string path, out object? oldValue);

        public delegate void SetDelegate(object item, object? internalState, string path, object? value, out object? oldValue);

        #endregion
    }
}