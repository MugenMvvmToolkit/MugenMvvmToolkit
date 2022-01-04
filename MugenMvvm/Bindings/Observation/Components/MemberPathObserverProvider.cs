using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Observation.Components
{
    public sealed class MemberPathObserverProvider : IMemberPathObserverProviderComponent, IHasPriority
    {
        [Preserve(Conditional = true)]
        public MemberPathObserverProvider()
        {
        }

        public HashSet<string>? ObservableMethods { get; set; }

        public int Priority { get; init; } = ObservationComponentPriority.MemberPathObserverProvider;

        public IMemberPathObserver? TryGetMemberPathObserver(IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is not MemberPathObserverRequest observerRequest)
                return null;

            var path = observerRequest.Path;
            var membersCount = path.Members.Count;
            if (observerRequest.ObservableMethodName != null && (ObservableMethods == null || ObservableMethods.Contains(observerRequest.ObservableMethodName)))
            {
                if (membersCount == 0)
                    return new MethodEmptyPathObserver(observerRequest.ObservableMethodName, target, observerRequest.MemberFlags);
                if (membersCount == 1)
                    return new MethodSinglePathObserver(observerRequest.ObservableMethodName, target, path, observerRequest.MemberFlags, observerRequest.Optional,
                        observerRequest.IsWeak);
                return new MethodMultiPathObserver(observerRequest.ObservableMethodName, target, path, observerRequest.MemberFlags, observerRequest.HasStablePath,
                    observerRequest.Optional, observerRequest.IsWeak);
            }

            if (membersCount == 0)
                return new EmptyPathObserver(target);
            if (membersCount == 1)
                return new SinglePathObserver(target, path, observerRequest.MemberFlags, observerRequest.Optional, observerRequest.IsWeak);

            if (observerRequest.Observable)
                return new MultiPathObserver(target, path, observerRequest.MemberFlags, observerRequest.HasStablePath, observerRequest.Optional, observerRequest.IsWeak);
            return new RootMultiPathObserver(target, path, observerRequest.MemberFlags, observerRequest.HasStablePath, observerRequest.Optional, observerRequest.IsWeak);
        }
    }
}