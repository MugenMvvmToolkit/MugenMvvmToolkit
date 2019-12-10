using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        #region Fields

        private readonly FuncIn<MemberPathObserverRequest, object, IMemberPathObserver?> _tryGetMemberPathObserverDelegate;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public MemberPathObserverProviderComponent()
        {
            ObservableRootMembers = new HashSet<string> {BindableMembers.Object.DataContext};
            _tryGetMemberPathObserverDelegate = TryGetMemberPathObserver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ObserverComponentPriority.ObserverProvider;

        public HashSet<string> ObservableRootMembers { get; }

        #endregion

        #region Implementation of interfaces

        public IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (_tryGetMemberPathObserverDelegate is FuncIn<TRequest, object, IMemberPathObserver> provider)
                return provider.Invoke(request, target);
            return null;
        }

        #endregion

        #region Methods

        private IMemberPathObserver? TryGetMemberPathObserver(in MemberPathObserverRequest request, object target)
        {
            var memberFlags = request.MemberFlags;
            var path = request.Path;
            var observableMethod = request.ObservableMethodName;
            var membersCount = path.Members.Count;
            if (!string.IsNullOrEmpty(observableMethod))
            {
                if (membersCount == 0)
                    return new ObservableMethodEmptyPathObserver(observableMethod!, target, memberFlags);
                if (membersCount == 1)
                    return new ObservableMethodSinglePathObserver(observableMethod!, target, path, memberFlags, request.Optional);
                return new ObservableMethodMultiPathObserver(observableMethod!, target, path, memberFlags, request.HasStablePath, request.Optional);
            }

            if (membersCount == 0)
                return new EmptyPathObserver(target);
            if (membersCount == 1)
                return new SinglePathObserver(target, path, memberFlags, request.Observable || ObservableRootMembers.Contains(path.Path), request.Optional);

            if (request.Observable)
                return new MultiPathObserver(target, path, memberFlags, request.HasStablePath, request.Optional);
            if (ObservableRootMembers.Contains(path.Members[0]))
                return new ObservableRootMultiPathObserver(target, path, memberFlags, request.HasStablePath, request.Optional);
            return new NonObservableMultiPathObserver(target, path, memberFlags, request.HasStablePath, request.Optional);
        }

        #endregion
    }
}