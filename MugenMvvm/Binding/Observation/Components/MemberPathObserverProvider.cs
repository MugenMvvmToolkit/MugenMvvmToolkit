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
        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathObserverProvider()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.MemberPathObserverProvider;

        public HashSet<string>? ObservableMethods { get; set; }

        #endregion

        #region Implementation of interfaces

        public IMemberPathObserver? TryGetMemberPathObserver(IObservationManager observationManager, object target, object request, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is MemberPathObserverRequest observerRequest))
                return null;

            var memberFlags = observerRequest.MemberFlags;
            var path = observerRequest.Path;
            var observableMethod = observerRequest.ObservableMethodName;
            var membersCount = path.Members.Count;
            if (!string.IsNullOrEmpty(observableMethod) && (ObservableMethods == null || ObservableMethods.Contains(observableMethod!)))
            {
                if (membersCount == 0)
                    return new MethodEmptyPathObserver(observableMethod!, target, memberFlags);
                if (membersCount == 1)
                    return new MethodSinglePathObserver(observableMethod!, target, path, memberFlags, observerRequest.Optional);
                return new MethodMultiPathObserver(observableMethod!, target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
            }

            if (membersCount == 0)
                return new EmptyPathObserver(target);
            if (membersCount == 1)
                return new SinglePathObserver(target, path, memberFlags, observerRequest.Optional);

            if (observerRequest.Observable)
                return new MultiPathObserver(target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
            return new RootMultiPathObserver(target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
        }

        #endregion
    }
}