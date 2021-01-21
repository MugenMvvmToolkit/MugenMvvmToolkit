using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class NonObservableMemberObserverDecorator : ComponentDecoratorBase<IObservationManager, IMemberObserverProviderComponent>, IMemberObserverProviderComponent
    {
        public NonObservableMemberObserverDecorator(int priority = ObservationComponentPriority.NonObservableMemberObserverDecorator) : base(priority)
        {
        }

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is IMemberInfo memberInfo && memberInfo.MemberFlags.HasFlag(MemberFlags.NonObservable))
                return default;
            return Components.TryGetMemberObserver(observationManager, type, member, metadata);
        }
    }
}