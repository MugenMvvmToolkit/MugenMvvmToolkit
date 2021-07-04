using System;
using System.Reflection;
using Avalonia;
using MugenMvvm.Avalonia.Extensions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Avalonia.Bindings
{
    public sealed class AvaloniaObjectObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        private static readonly Func<object, object?, MemberListenerCollection> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        private readonly IAttachedValueManager? _attachedValueManager;
        private readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> _memberObserverHandler;

        public AvaloniaObjectObserverProvider(IAttachedValueManager? attachedValueManager = null)
        {
            _attachedValueManager = attachedValueManager;
            _memberObserverHandler = TryObserve;
        }

        public int Priority { get; init; } = ObservationComponentPriority.PropertyChangedObserverProvider;

        private static MemberListenerCollection CreateWeakPropertyListener(object item, object? _)
        {
            var listener = new MemberListenerCollection();
            ((IAvaloniaObject) item).PropertyChanged += listener.RaisePropertyChanged;
            return listener;
        }

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is PropertyInfo p && !p.IsStatic())
                return TryGetMemberObserver(p.Name, type);
            if (member is IAccessorMemberInfo accessor && !accessor.MemberFlags.HasFlag(MemberFlags.Static))
                return TryGetMemberObserver(accessor.Name, type);
            return default;
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
            if (typeof(IAvaloniaObject).IsAssignableFrom(type))
                return new MemberObserver(_memberObserverHandler, member);
            return default;
        }
    }
}