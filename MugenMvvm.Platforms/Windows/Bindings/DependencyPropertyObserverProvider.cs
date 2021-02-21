using System;
using System.Windows;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Windows.Bindings
{
    public sealed class DependencyPropertyObserverProvider : IMemberObserverProviderComponent, IHasPriority
    {
        private static readonly Func<object?, object, IEventListener, IReadOnlyMetadataContext?, ActionToken> MemberObserverHandler = Observe;

        public int Priority { get; set; }

        private static MemberObserver GetMemberObserver(DependencyProperty dp) => new(MemberObserverHandler, dp.Name);

        private static ActionToken Observe(object? t, object member, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            var l = new DependencyPropertyListener();
            l.SetListener(listener, t!, (string) member);
            return new ActionToken((o, _) => ((DependencyPropertyListener) o!).Clear(), l);
        }

        public MemberObserver TryGetMemberObserver(IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            if (member is DependencyProperty dp)
                return GetMemberObserver(dp);
            if (member is IMemberInfo {UnderlyingMember: DependencyProperty p})
                return GetMemberObserver(p);
            return default;
        }
    }
}