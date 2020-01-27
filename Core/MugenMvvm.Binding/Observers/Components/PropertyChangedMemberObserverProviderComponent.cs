using System;
using System.ComponentModel;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class PropertyChangedMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority //todo add static property changed listener
    {
        #region Fields

        private readonly IAttachedValueProvider? _attachedValueProvider;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;
        private readonly FuncIn<MemberObserverRequest, Type, MemberObserver> _tryGetMemberObserverRequestDelegate;

        private static readonly Func<INotifyPropertyChanged, object?, WeakPropertyChangedListener> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedMemberObserverProviderComponent(IAttachedValueProvider? attachedValueProvider = null)
        {
            _attachedValueProvider = attachedValueProvider;
            _tryGetMemberObserverRequestDelegate = TryGetMemberObserver;
            _memberObserverHandler = TryObserve;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.PropertyChanged;

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TMember>())
            {
                if (_tryGetMemberObserverRequestDelegate is FuncIn<TMember, Type, MemberObserver> provider)
                    return provider.Invoke(member, type);
                return default;
            }

            if (member is PropertyInfo property)
            {
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(type) && !property.IsStatic())
                    return new MemberObserver(_memberObserverHandler, property.Name);
                return default;
            }

            if (member is string stringMember)
                return TryGetMemberObserver(stringMember, type);
            return default;
        }

        #endregion

        #region Methods

        private ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;
            return _attachedValueProvider
                .DefaultIfNull()
                .GetOrAdd((INotifyPropertyChanged) target, BindingInternalConstant.PropertyChangedObserverMember, null, CreateWeakPropertyListenerDelegate)
                .Add(listener, (string) member);
        }

        private MemberObserver TryGetMemberObserver(in MemberObserverRequest request, Type type)
        {
            if (request.ReflectionMember is PropertyInfo)
                return TryGetMemberObserver(request.Path, type);
            return default;
        }

        private MemberObserver TryGetMemberObserver(string member, Type type)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                return new MemberObserver(_memberObserverHandler, member);
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
                var propertyName = (string) state!;
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

                if (_listeners.Length / (float) (_size - _removedSize) <= 2)
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