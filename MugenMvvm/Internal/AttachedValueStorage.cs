using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct AttachedValueStorage
    {
        #region Fields

        private readonly object _item;
        private object? _state;
        private readonly IAttachedValueStorageManager _storageManager;

        #endregion

        #region Constructors

        public AttachedValueStorage(object item, IAttachedValueStorageManager storageManager, object? state)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(storageManager, nameof(storageManager));
            _item = item;
            _storageManager = storageManager;
            _state = state;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _storageManager == null;

        #endregion

        #region Methods

        public AttachedValueStorage Decorate<TState>(TState state, DecorateDelegate<TState> decorator)
        {
            EnsureInitialized();
            return decorator(_item, _storageManager, _state, state);
        }

        public int GetCount()
        {
            EnsureInitialized();
            return _storageManager.GetCount(_item, ref _state);
        }

        public ItemOrList<KeyValuePair<string, object?>, IReadOnlyList<KeyValuePair<string, object?>>> GetValues<TState>(TState state = default,
            Func<object, KeyValuePair<string, object?>, TState, bool>? predicate = null)
        {
            EnsureInitialized();
            return _storageManager.GetValues(_item, ref _state, state, predicate);
        }

        public bool Contains(string path)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager != null && _storageManager.Contains(_item, ref _state, path);
        }

        public bool TryGet(string path, out object? value)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager.TryGet(_item, ref _state, path, out value);
        }

        public TValue AddOrUpdate<TValue, TState>(string path, TValue addValue, TState state, UpdateValueDelegate<object, TValue, TValue, TState, TValue> updateValueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            EnsureInitialized();
            return _storageManager.AddOrUpdate(_item, ref _state, path, addValue, state, updateValueFactory);
        }

        public TValue AddOrUpdate<TValue, TState>(string path, TState state, Func<object, TState, TValue> addValueFactory, UpdateValueDelegate<object, TValue, TState, TValue> updateValueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            EnsureInitialized();
            return _storageManager.AddOrUpdate(_item, ref _state, path, state, addValueFactory, updateValueFactory);
        }

        public TValue GetOrAdd<TValue, TState>(string path, TState state, Func<object, TState, TValue> valueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            EnsureInitialized();
            return _storageManager.GetOrAdd(_item, ref _state, path, state, valueFactory);
        }

        public TValue GetOrAdd<TValue>(string path, TValue value)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager.GetOrAdd(_item, ref _state, path, value);
        }

        public void Set(string path, object? value, out object? oldValue)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            _storageManager.Set(_item, ref _state, path, value, out oldValue);
        }

        public bool Remove(string path, out object? oldValue)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager.Remove(_item, ref _state, path, out oldValue);
        }

        public bool Clear()
        {
            EnsureInitialized();
            return _storageManager.Clear(_item, ref _state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureInitialized()
        {
            if (_storageManager == null)
                ExceptionManager.ThrowObjectNotInitialized(nameof(AttachedValueStorage));
        }

        #endregion

        #region Nested Types

        public delegate AttachedValueStorage DecorateDelegate<in TState>(object item, IAttachedValueStorageManager storageManager, object? internalState, TState state);

        #endregion
    }
}