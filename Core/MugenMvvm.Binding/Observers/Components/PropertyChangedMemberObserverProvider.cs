using System;
using System.ComponentModel;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class PropertyChangedMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueProvider? _attachedValueProvider;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;

        private static readonly Func<INotifyPropertyChanged, object?, PropertyChangedListenerCollection> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedMemberObserverProvider(IAttachedValueProvider? attachedValueProvider = null)
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
            if (TypeChecker.IsValueType<TMember>())
                return default;

            if (member is PropertyInfo p && !p.IsStatic())
                return TryGetMemberObserver(p.Name, type);
            if (member is IAccessorMemberInfo accessor && !accessor.AccessModifiers.HasFlagEx(MemberFlags.Static))
                return TryGetMemberObserver(accessor.Name, type);
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
                .GetOrAdd((INotifyPropertyChanged)target, BindingInternalConstant.PropertyChangedObserverMember, null, CreateWeakPropertyListenerDelegate)
                .Add(listener, (string)member);
        }

        private MemberObserver TryGetMemberObserver(string member, Type type)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                return new MemberObserver(_memberObserverHandler, member);
            return default;
        }

        private static PropertyChangedListenerCollection CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object? _)
        {
            var listener = new PropertyChangedListenerCollection();
            propertyChanged.PropertyChanged += listener.Raise;
            return listener;
        }

        #endregion
    }
}