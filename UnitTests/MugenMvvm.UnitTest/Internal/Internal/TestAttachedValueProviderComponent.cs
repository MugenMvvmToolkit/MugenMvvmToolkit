using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestAttachedValueProviderComponent : IAttachedValueProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _manager;

        #endregion

        #region Constructors

        public TestAttachedValueProviderComponent(IAttachedValueManager? manager)
        {
            _manager = manager;
        }

        #endregion

        #region Properties

        public Func<object, IReadOnlyMetadataContext?, bool>? IsSupported { get; set; }

        public Func<object, object?, Func<object, KeyValuePair<string, object?>, object?, bool>?, ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>>>? TryGetValues { get; set; }

        public Func<object, string, object?>? TryGet { get; set; }

        public Func<object, string, bool>? Contains { get; set; }

        public Func<object, string, object?, object?, UpdateValueDelegate<object, object?, object?, object?, object?>, object?>? AddOrUpdate { get; set; }

        public Func<object, string, object?, Func<object, object?, object?>, UpdateValueDelegate<object, object?, object?, object?>, object?>? AddOrUpdate1 { get; set; }

        public Func<object, string, object?, object?>? GetOrAdd { get; set; }

        public Func<object, string, object?, Func<object, object?, object?>, object?>? GetOrAdd1 { get; set; }

        public SetDelegate? Set { get; set; }

        public ClearDelegate? ClearKey { get; set; }

        public Func<object, bool>? Clear { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IAttachedValueProviderComponent.IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return IsSupported?.Invoke(item, metadata) ?? false;
        }

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> IAttachedValueProviderComponent.GetValues(IAttachedValueManager attachedValueManager, object item,
            Func<object, KeyValuePair<string, object?>, object?, bool>? predicate, object? state)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return TryGetValues!.Invoke(item, state, predicate);
        }

        bool IAttachedValueProviderComponent.TryGet(IAttachedValueManager attachedValueManager, object item, string path, out object? value)
        {
            _manager?.ShouldEqual(attachedValueManager);
            var result = TryGet!.Invoke(item, path);
            if (result == null)
            {
                value = default!;
                return false;
            }

            value = result;
            return true;
        }

        bool IAttachedValueProviderComponent.Contains(IAttachedValueManager attachedValueManager, object item, string path)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return Contains!.Invoke(item, path);
        }

        object? IAttachedValueProviderComponent.AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, object? addValue,
            UpdateValueDelegate<object, object?, object?, object?, object?> updateValueFactory, object? state)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return AddOrUpdate!.Invoke(item, path, addValue, state, updateValueFactory);
        }

        object? IAttachedValueProviderComponent.AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> addValueFactory,
            UpdateValueDelegate<object, object?, object?, object?> updateValueFactory, object? state)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return AddOrUpdate1!.Invoke(item, path, state, addValueFactory, updateValueFactory);
        }

        object? IAttachedValueProviderComponent.GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, object? value)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return GetOrAdd!.Invoke(item, path, value);
        }

        object? IAttachedValueProviderComponent.GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> valueFactory, object? state)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return GetOrAdd1!.Invoke(item, path, state, valueFactory)!;
        }

        void IAttachedValueProviderComponent.Set(IAttachedValueManager attachedValueManager, object item, string path, object? value, out object? oldValue)
        {
            _manager?.ShouldEqual(attachedValueManager);
            Set!.Invoke(item, path, value!, out oldValue);
        }

        bool IAttachedValueProviderComponent.Clear(IAttachedValueManager attachedValueManager, object item, string path, out object? oldValue)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return ClearKey!.Invoke(item, path, out oldValue);
        }

        bool IAttachedValueProviderComponent.Clear(IAttachedValueManager attachedValueManager, object item)
        {
            _manager?.ShouldEqual(attachedValueManager);
            return Clear!.Invoke(item);
        }

        #endregion

        #region Nested types

        public delegate bool ClearDelegate(object item, string path, out object? oldValue);

        public delegate void SetDelegate(object item, string path, object? value, out object? oldValue);

        #endregion
    }
}