using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Binding.Observers.PathObservers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathObserverProviderComponent()
        {
            ObservableRootMembers = new HashSet<string> {BindableMembers.Object.DataContext};
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.ObserverProvider;

        public HashSet<string> ObservableRootMembers { get; }

        #endregion

        #region Implementation of interfaces

        public IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(TRequest) != typeof(MemberPathObserverRequest))
                return null;

            var observerRequest = MugenExtensions.CastGeneric<TRequest, MemberPathObserverRequest>(request);
            var memberFlags = observerRequest.MemberFlags;
            var path = observerRequest.Path;
            var observableMethod = observerRequest.ObservableMethodName;
            var membersCount = path.Members.Count;
            if (!string.IsNullOrEmpty(observableMethod))
            {
                if (membersCount == 0)
                    return new ObservableMethodEmptyPathObserver(observableMethod!, target, memberFlags);
                if (membersCount == 1)
                    return new ObservableMethodSinglePathObserver(observableMethod!, target, path, memberFlags, observerRequest.Optional);
                return new ObservableMethodMultiPathObserver(observableMethod!, target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
            }

            if (membersCount == 0)
                return new EmptyPathObserver(target);
            if (membersCount == 1)
                return new SinglePathObserver(target, path, memberFlags, observerRequest.Observable || ObservableRootMembers.Contains(path.Path), observerRequest.Optional);

            if (observerRequest.Observable)
                return new MultiPathObserver(target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
            if (ObservableRootMembers.Contains(path.Members[0]))
                return new ObservableRootMultiPathObserver(target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
            return new NonObservableMultiPathObserver(target, path, memberFlags, observerRequest.HasStablePath, observerRequest.Optional);
        }

        #endregion
    }
}