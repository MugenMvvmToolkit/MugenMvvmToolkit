using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IAttachedValueManager : IComponentOwner<IAttachedValueManager>
    {
        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(object item, Func<object, KeyValuePair<string, object?>, object?, bool>? predicate = null, object? state = null);

        bool TryGet(object item, string path, out object? value);

        bool Contains(object item, string path);

        object? AddOrUpdate(object item, string path, object? addValue, UpdateValueDelegate<object, object?, object?, object?, object?> updateValueFactory, object? state = null);

        object? AddOrUpdate(object item, string path, Func<object, object?, object?> addValueFactory, UpdateValueDelegate<object, object?, object?, object?> updateValueFactory, object? state = null);

        object? GetOrAdd(object item, string path, Func<object, object?, object?> valueFactory, object? state = null);

        object? GetOrAdd(object item, string path, object? value);

        void Set(object item, string path, object? value, out object? oldValue);

        bool Clear(object item, string path, out object? oldValue);

        bool Clear(object item);
    }
}