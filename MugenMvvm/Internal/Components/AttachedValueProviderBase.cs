using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal.Components
{
    public abstract class AttachedValueProviderBase : IAttachedValueProviderComponent
    {
        #region Properties

        protected virtual bool ClearOnEmpty => false;

        #endregion

        #region Implementation of interfaces

        public abstract bool IsSupported(IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata);

        public virtual ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues(IAttachedValueManager attachedValueManager, object item,
            Func<object, KeyValuePair<string, object?>, object?, bool>? predicate, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return default;
            lock (dictionary)
            {
                if (dictionary.Count == 0)
                    return default;

                if (predicate == null)
                {
                    if (dictionary.Count == 1)
                        return dictionary.FirstOrDefault();
                    return ItemOrList.FromListToReadOnly(new List<KeyValuePair<string, object?>>(dictionary));
                }

                var result = ItemOrListEditor.Get<KeyValuePair<string, object?>>(pair => pair.Key == null);
                foreach (var keyValue in dictionary)
                {
                    if (predicate(item, keyValue, state))
                        result.Add(keyValue);
                }

                return result.ToItemOrList<IReadOnlyList<KeyValuePair<string, object?>>>();
            }
        }

        public virtual bool TryGet(IAttachedValueManager attachedValueManager, object item, string path, out object? value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
            {
                value = default!;
                return false;
            }

            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var result))
                {
                    value = result;
                    return true;
                }

                value = default!;
                return false;
            }
        }

        public virtual bool Contains(IAttachedValueManager attachedValueManager, object item, string path)
        {
            Should.NotBeNull(item, nameof(item));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
                return false;
            lock (dictionary)
            {
                return dictionary.ContainsKey(path);
            }
        }

        public virtual object? AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, object? addValue, UpdateValueDelegate<object, object?, object?, object?, object?> updateValueFactory,
            object? state)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValue, value!, state);
                    dictionary[path] = value;
                    return value;
                }

                dictionary.Add(path, addValue);
                return addValue;
            }
        }

        public virtual object? AddOrUpdate(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> addValueFactory,
            UpdateValueDelegate<object, object?, object?, object?> updateValueFactory, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var value))
                {
                    value = updateValueFactory(item, addValueFactory, value, state);
                    dictionary[path] = value;
                    return value!;
                }

                value = addValueFactory(item, state);
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual object? GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, Func<object, object?, object?> valueFactory, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return oldValue;
                oldValue = valueFactory(item, state);
                dictionary.Add(path, oldValue);
                return oldValue;
            }
        }


        public virtual object? GetOrAdd(IAttachedValueManager attachedValueManager, object item, string path, object? value)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(path, out var oldValue))
                    return oldValue;
                dictionary.Add(path, value);
                return value;
            }
        }

        public virtual void Set(IAttachedValueManager attachedValueManager, object item, string path, object? value, out object? oldValue)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, false)!;
            lock (dictionary)
            {
                dictionary.TryGetValue(path, out oldValue);
                dictionary[path] = value;
            }
        }

        public virtual bool Remove(IAttachedValueManager attachedValueManager, object item, string path, out object? oldValue)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(path, nameof(path));
            var dictionary = GetAttachedDictionary(item, true);
            if (dictionary == null)
            {
                oldValue = null;
                return false;
            }

            bool clear;
            bool removed;
            lock (dictionary)
            {
                removed = dictionary.TryGetValue(path!, out oldValue) && dictionary.Remove(path!);
                clear = removed && dictionary.Count == 0;
            }

            if (clear && ClearOnEmpty)
                return ClearInternal(item);
            return removed;
        }

        public virtual bool Clear(IAttachedValueManager attachedValueManager, object item)
        {
            Should.NotBeNull(item, nameof(item));
            return ClearInternal(item);
        }

        #endregion

        #region Methods

        protected abstract IDictionary<string, object?>? GetAttachedDictionary(object item, bool optional);

        protected abstract bool ClearInternal(object item);

        #endregion
    }
}