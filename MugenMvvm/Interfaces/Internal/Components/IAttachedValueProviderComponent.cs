using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IAttachedValueProviderComponent : IComponent<IAttachedValueManager>//todo review
    {
        bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata);

        ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(IAttachedValueManager attachedValueManager, object item, Func<object, KeyValuePair<string, object?>, object?, bool>? predicate, object? state);

        bool TryGet(IAttachedValueManager attachedValueManager, object item, string path, out object? value);

        bool Contains(IAttachedValueManager attachedValueManager, object item, string path);

        object? AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, object? addValue, UpdateValueDelegate<object, object?, object?, object?, object?> updateValueFactory, object? state);

        object? AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> addValueFactory, UpdateValueDelegate<object, object?, object?, object?> updateValueFactory, object? state);

        object? GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> valueFactory, object? state);

        object? GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, object? value);

        void Set(IAttachedValueManager attachedValueManager, object item, string path, object? value, out object? oldValue);

        bool Remove(IAttachedValueManager attachedValueManager, object item, string path, out object? oldValue);

        bool Clear(IAttachedValueManager attachedValueManager, object item);
    }
}