using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct AttachedValueStorage
    {
        #region Fields

        private readonly object? _item;
        private object? _state;
        private readonly IAttachedValueStorageManager? _storageManager;

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

        public void Deconstruct(out IAttachedValueStorageManager storageManager, out object item, out object? state)
        {
            EnsureInitialized();
            storageManager = _storageManager!;
            item = _item!;
            state = _state;
        }

        public int GetCount()
        {
            EnsureInitialized();
            return _storageManager!.GetCount(_item!, ref _state);
        }

        public ItemOrIReadOnlyList<KeyValuePair<string, object?>> GetValues() => GetValues<object?>(null, null);

        public ItemOrIReadOnlyList<KeyValuePair<string, object?>> GetValues<TState>(TState state, Func<object, string, object?, TState, bool>? predicate)
        {
            EnsureInitialized();
            return _storageManager!.GetValues(_item!, state, predicate, ref _state);
        }

        public bool Contains(string path)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager!.Contains(_item!, path, ref _state);
        }

        public bool TryGet(string path, out object? value)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager!.TryGet(_item!, path, ref _state, out value);
        }

        public TValue AddOrUpdate<TValue, TState>(string path, TValue addValue, TState state, Func<object, string, TValue, TState, TValue> updateValueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            EnsureInitialized();
            return _storageManager!.AddOrUpdate(_item!, path, addValue, state, updateValueFactory, ref _state);
        }

        public TValue AddOrUpdate<TValue, TState>(string path, TState state, Func<object, TState, TValue> addValueFactory, Func<object, string, TValue, TState, TValue> updateValueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(addValueFactory, nameof(addValueFactory));
            Should.NotBeNull(updateValueFactory, nameof(updateValueFactory));
            EnsureInitialized();
            return _storageManager!.AddOrUpdate(_item!, path, state, addValueFactory, updateValueFactory, ref _state);
        }

        public TValue GetOrAdd<TValue, TState>(string path, TState state, Func<object, TState, TValue> valueFactory)
        {
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(valueFactory, nameof(valueFactory));
            EnsureInitialized();
            return _storageManager!.GetOrAdd(_item!, path, state, valueFactory, ref _state);
        }

        public TValue GetOrAdd<TValue>(string path, TValue value)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager!.GetOrAdd(_item!, path, value, ref _state);
        }

        public void Set(string path, object? value) => Set(path, value, out _);

        public void Set(string path, object? value, out object? oldValue)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            _storageManager!.Set(_item!, path, value, ref _state, out oldValue);
        }

        public bool Remove(string path) => Remove(path, out _);

        public bool Remove(string path, out object? oldValue)
        {
            Should.NotBeNull(path, nameof(path));
            EnsureInitialized();
            return _storageManager!.Remove(_item!, path, ref _state, out oldValue);
        }

        public bool Clear()
        {
            EnsureInitialized();
            return _storageManager!.Clear(_item!, ref _state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureInitialized()
        {
            if (_storageManager == null)
                ExceptionManager.ThrowObjectNotInitialized(nameof(AttachedValueStorage));
        }

        #endregion
    }
}