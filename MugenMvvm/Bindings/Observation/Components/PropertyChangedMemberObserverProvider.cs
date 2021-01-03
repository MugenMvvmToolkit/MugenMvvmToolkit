using System;
using System.ComponentModel;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class PropertyChangedMemberObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;

        public static readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> MemberObserverHolderHandler = TryObserveHolder;
        private static readonly Func<object, object?, MemberListenerCollection> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedMemberObserverProvider(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _memberObserverHandler = TryObserve;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.PropertyChangedObserverProvider;

        #endregion

        #region Implementation of interfaces

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is PropertyInfo p && !p.IsStatic())
                return TryGetMemberObserver(p.Name, type);
            if (member is IAccessorMemberInfo accessor && !accessor.AccessModifiers.HasFlag(MemberFlags.Static))
                return TryGetMemberObserver(accessor.Name, type);
            return default;
        }

        #endregion

        #region Methods

        private static ActionToken TryObserveHolder(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;

            return (((IValueHolder<MemberListenerCollection>) target).Value ??= new MemberListenerCollection()).Add(listener, (string) member);
        }

        private ActionToken TryObserve(object? target, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return default;
            return target.AttachedValues(metadata, _attachedValueManager)
                .GetOrAdd(BindingInternalConstant.PropertyChangedObserverMember, null, CreateWeakPropertyListenerDelegate)
                .Add(listener, (string) member);
        }

        private MemberObserver TryGetMemberObserver(string member, Type type)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(type))
                return new MemberObserver(_memberObserverHandler, member);
            if (typeof(IValueHolder<MemberListenerCollection>).IsAssignableFrom(type))
                return new MemberObserver(MemberObserverHolderHandler, member);
            return default;
        }

        private static MemberListenerCollection CreateWeakPropertyListener(object item, object? _)
        {
            var listener = new MemberListenerCollection();
            ((INotifyPropertyChanged) item).PropertyChanged += listener.RaisePropertyChanged;
            return listener;
        }

        #endregion
    }
}