﻿using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class MemberPathObserverProviderComponent : IMemberPathObserverProviderComponent, IHasPriority
    {
        #region Fields

        private static readonly FuncEx<MemberPathObserverRequest, object, IReadOnlyMetadataContext?, IMemberPathObserver?> TryGetMemberPathObserverDelegate =
            TryGetMemberPathObserver;

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object target, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TryGetMemberPathObserverDelegate is FuncEx<TRequest, object, IReadOnlyMetadataContext?, IMemberPathObserver> provider)
                return provider.Invoke(request, target, metadata);
            return null;
        }

        #endregion

        #region Methods

        private static IMemberPathObserver? TryGetMemberPathObserver(in MemberPathObserverRequest request, object target, IReadOnlyMetadataContext? metadata)
        {
            var memberFlags = request.MemberFlags;
            var path = request.Path;
            var observableMethod = request.ObservableMethodName;
            if (string.IsNullOrEmpty(observableMethod))
            {
                if (path.IsSingle)
                    return new SinglePathObserver(target, path, memberFlags, request.Observable, request.Optional);
                if (path.Members.Length == 0)
                    return new EmptyPathObserver(target);
                return new MultiPathObserver(target, path, memberFlags, request.HasStablePath, request.Observable, request.Optional);
            }

            if (path.IsSingle)
                return new MethodSinglePathObserver(observableMethod!, target, path, memberFlags, request.Optional);
            if (path.Members.Length == 0)
                return new MethodEmptyPathObserver(observableMethod!, target, memberFlags);
            return new MethodMultiPathObserver(observableMethod!, target, path, memberFlags, request.HasStablePath, request.Optional);
        }

        #endregion
    }
}