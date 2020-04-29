using System;
using System.ComponentModel;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
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

        private static readonly Func<INotifyPropertyChanged, object?, WeakPropertyChangedListener> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedMemberObserverProviderComponent(IAttachedValueProvider? attachedValueProvider = null)
        {
            _attachedValueProvider = attachedValueProvider;
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
                if (typeof(TMember) == typeof(MemberObserverRequest))
                {
                    var request = MugenExtensions.CastGeneric<TMember, MemberObserverRequest>(member);
                    if (request.ReflectionMember is PropertyInfo)
                        return TryGetMemberObserver(request.Path, type);
                }

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

        private MemberObserver TryGetMemberObserver(string member, Type type)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                return new MemberObserver(_memberObserverHandler, member);
            return default;
        }

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object? _)
        {
            var listener = new WeakPropertyChangedListener();
            propertyChanged.PropertyChanged += listener.Raise;
            return listener;
        }

        #endregion
    }
}