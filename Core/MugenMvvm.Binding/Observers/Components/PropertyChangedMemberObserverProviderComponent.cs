using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class PropertyChangedMemberObserverProviderComponent : IMemberObserverProviderComponent, MemberObserver.IHandler, IHasPriority //todo add static property changed listener
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly FuncEx<PropertyInfo, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverPropertyDelegate;
        private readonly FuncEx<string, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverStringDelegate;
        private readonly FuncEx<MemberObserverRequest, Type, IReadOnlyMetadataContext?, MemberObserver> _tryGetMemberObserverRequestDelegate;

        private static readonly Func<INotifyPropertyChanged, object?, WeakPropertyChangedListener> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedMemberObserverProviderComponent(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _tryGetMemberObserverStringDelegate = TryGetMemberObserver;
            _tryGetMemberObserverPropertyDelegate = TryGetMemberObserver;
            _tryGetMemberObserverRequestDelegate = TryGetMemberObserver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 1;

        #endregion

        #region Implementation of interfaces

        ActionToken MemberObserver.IHandler.TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;
            return _attachedValueManager
                .ServiceIfNull()
                .GetOrAdd((INotifyPropertyChanged)target, BindingInternalConstants.PropertyChangedObserverMember, null, CreateWeakPropertyListenerDelegate)
                .Add(listener, (string)member);
        }

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetMemberObserverPropertyDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider1)
                return provider1.Invoke(member, type, metadata);
            if (_tryGetMemberObserverStringDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider2)
                return provider2.Invoke(member, type, metadata);
            if (_tryGetMemberObserverRequestDelegate is FuncEx<TMember, Type, IReadOnlyMetadataContext?, MemberObserver> provider3)
                return provider3.Invoke(member, type, metadata);
            return default;
        }

        #endregion

        #region Methods

        private MemberObserver TryGetMemberObserver(in MemberObserverRequest request, Type type, IReadOnlyMetadataContext? metadata)
        {
            if (request.Member is PropertyInfo)
                return TryGetMemberObserver(request.Path, type, metadata);
            return default;
        }

        private MemberObserver TryGetMemberObserver(in PropertyInfo member, Type type, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type) && !member.IsStatic())
                return new MemberObserver(this, member.Name);
            return default;
        }

        private MemberObserver TryGetMemberObserver(in string member, Type type, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                return new MemberObserver(this, member);
            return default;
        }

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object? _)
        {
            var listener = new WeakPropertyChangedListener();
            propertyChanged.PropertyChanged += listener.Handle;
            return listener;
        }

        #endregion

        #region Nested types

        private sealed class WeakPropertyChangedListener : ActionToken.IHandler
        {
            #region Fields

            private KeyValuePair<WeakEventListener, string>[] _listeners;
            private ushort _removedSize;
            private ushort _size;

            #endregion

            #region Constructors

            public WeakPropertyChangedListener()
            {
                _listeners = Default.EmptyArray<KeyValuePair<WeakEventListener, string>>();
            }

            #endregion

            #region Implementation of interfaces

            void ActionToken.IHandler.Invoke(object? state1, object? state2)
            {
                var propertyName = (string)state2!;
                for (var i = 0; i < _listeners.Length; i++)
                {
                    var pair = _listeners[i];
                    if (!pair.Key.IsEmpty && pair.Value == propertyName && ReferenceEquals(pair.Key.Target, state1))
                    {
                        ++_removedSize;
                        _listeners[i] = default;
                        return;
                    }
                }
            }

            #endregion

            #region Methods

            public void Handle(object sender, PropertyChangedEventArgs args)
            {
                var hasDeadRef = false;
                var listeners = _listeners;
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (i >= _size)
                        break;
                    var pair = listeners[i];
                    if (pair.Key.IsEmpty)
                    {
                        hasDeadRef = true;
                        continue;
                    }

                    if (MugenExtensions.MemberNameEqual(args.PropertyName, pair.Value, true))
                    {
                        if (!pair.Key.TryHandle(sender, args))
                            hasDeadRef = true;
                    }
                }

                if (hasDeadRef)
                    Cleanup();
            }

            public ActionToken Add(IEventListener target, string path)
            {
                var weakItem = target.ToWeak();
                if (_listeners.Length == 0)
                {
                    _listeners = new[] { new KeyValuePair<WeakEventListener, string>(weakItem, path) };
                    _size = 1;
                    _removedSize = 0;
                }
                else
                {
                    if (_removedSize == 0)
                    {
                        if (_size == _listeners.Length)
                            EventListenerCollection.EnsureCapacity(ref _listeners, _size, _size + 1);
                        _listeners[_size++] = new KeyValuePair<WeakEventListener, string>(weakItem, path);
                    }
                    else
                    {
                        for (var i = 0; i < _size; i++)
                        {
                            if (_listeners[i].Key.IsEmpty)
                            {
                                _listeners[i] = new KeyValuePair<WeakEventListener, string>(weakItem, path);
                                --_removedSize;
                                break;
                            }
                        }
                    }
                }

                return new ActionToken(this, weakItem.Target, path);
            }

            private void Cleanup()
            {
                var size = _size;
                _size = 0;
                _removedSize = 0;
                for (var i = 0; i < size; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Key.IsAlive)
                        _listeners[_size++] = reference;
                }

                if (_size == 0)
                    _listeners = Default.EmptyArray<KeyValuePair<WeakEventListener, string>>();
                else if (_listeners.Length / (float)_size > 2)
                {
                    var listeners = new KeyValuePair<WeakEventListener, string>[_size + (_size >> 2)];
                    Array.Copy(_listeners, 0, listeners, 0, _size);
                    _listeners = listeners;
                }
            }

            #endregion
        }

        #endregion
    }
}