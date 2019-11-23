using System;
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

        public int Priority { get; set; } = ObserverComponentPriority.PropertyChanged;

        #endregion

        #region Implementation of interfaces

        ActionToken MemberObserver.IHandler.TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;
            return _attachedValueManager
                .DefaultIfNull()
                .GetOrAdd((INotifyPropertyChanged)target, BindingInternalConstant.PropertyChangedObserverMember, null, CreateWeakPropertyListenerDelegate)
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
            if (request.ReflectionMember is PropertyInfo)
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

            private WeakEventListener<string>[] _listeners;
            private ushort _removedSize;
            private ushort _size;

            #endregion

            #region Constructors

            public WeakPropertyChangedListener()
            {
                _listeners = Default.EmptyArray<WeakEventListener<string>>();
            }

            #endregion

            #region Implementation of interfaces

            void ActionToken.IHandler.Invoke(object? target, object? state)
            {
                var propertyName = (string)state!;
                var listeners = _listeners;
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    var listener = listeners[i];
                    if (ReferenceEquals(listener.Target, target) && listener.State == propertyName)
                    {
                        if (RemoveAt(listeners, i))
                            TrimIfNeed();
                        break;
                    }
                }
            }

            #endregion

            #region Methods

            public void Handle(object sender, PropertyChangedEventArgs args)
            {
                var hasDeadRef = false;
                var listeners = _listeners;
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    var listener = listeners[i];
                    if (!listener.IsEmpty && MugenExtensions.MemberNameEqual(args.PropertyName, listener.State, true) && !listener.TryHandle(sender, args) && RemoveAt(listeners, i))
                        hasDeadRef = true;
                }

                if (hasDeadRef)
                    TrimIfNeed();
            }

            public ActionToken Add(IEventListener target, string path)
            {
                var weakItem = target.ToWeak(path);
                if (_removedSize == 0)
                {
                    if (_size == _listeners.Length)
                        Array.Resize(ref _listeners, _size + 2);
                    _listeners[_size++] = weakItem;
                }
                else
                {
                    for (var i = 0; i < _size; i++)
                    {
                        if (_listeners[i].IsEmpty)
                        {
                            _listeners[i] = weakItem;
                            --_removedSize;
                            break;
                        }
                    }
                }

                return new ActionToken(this, weakItem.Target, path);
            }

            private bool RemoveAt(WeakEventListener<string>[] listeners, int index)
            {
                if (!ReferenceEquals(listeners, _listeners))
                    return false;

                listeners[index] = default;
                if (index == _size - 1)
                    --_size;
                else
                    ++_removedSize;
                return true;
            }

            private void TrimIfNeed()
            {
                if (_size == _removedSize)
                {
                    _size = 0;
                    _removedSize = 0;
                    _listeners = Default.EmptyArray<WeakEventListener<string>>();
                    return;
                }

                if (_listeners.Length / (float)(_size - _removedSize) <= 2)
                    return;

                var size = _size;
                _size = 0;
                _removedSize = 0;
                for (var i = 0; i < size; i++)
                {
                    var reference = _listeners[i];
                    _listeners[i] = default;
                    if (WeakEventListener.GetIsAlive(reference))
                        _listeners[_size++] = reference;
                }

                if (_size == 0)
                {
                    _listeners = Default.EmptyArray<WeakEventListener<string>>();
                    return;
                }

                var capacity = _size + 1;
                if (size != capacity)
                    Array.Resize(ref _listeners, capacity);
            }

            #endregion
        }

        #endregion
    }
}