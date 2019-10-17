using System;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
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

        public IMemberPathObserver? TryGetMemberPathObserver<TRequest>(object source, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TryGetMemberPathObserverDelegate is FuncEx<TRequest, object, IReadOnlyMetadataContext?, IMemberPathObserver> provider)
                return provider.Invoke(request, provider, metadata);
            return null;
        }

        #endregion

        #region Methods

        private static IMemberPathObserver? TryGetMemberPathObserver(in MemberPathObserverRequest request, object source, IReadOnlyMetadataContext? metadata)
        {
            var memberFlags = request.MemberFlags;
            var isStatic = memberFlags.HasFlagEx(MemberFlags.Static);
            object sourceValue;
            if (isStatic && source is Type)
                sourceValue = source;
            else
                sourceValue = source.ToWeakReference();

            var path = request.Path;
            var observableMethod = request.ObservableMethodName;
            if (string.IsNullOrEmpty(observableMethod))
            {
                if (path.IsSingle)
                    return new SinglePathObserver(sourceValue, path, memberFlags, request.Observable, request.Optional);
                if (path.Members.Length == 0)
                    return new EmptyPathObserver((IWeakReference) sourceValue);
                return new MultiPathObserver(sourceValue, path, memberFlags, request.HasStablePath, request.Observable, request.Optional);
            }

            if (path.IsSingle)
                return new MethodSinglePathObserver(observableMethod, sourceValue, path, memberFlags, request.Optional);
            if (path.Members.Length == 0)
                return new MethodEmptyPathObserver(observableMethod, sourceValue, memberFlags);
            return new MethodMultiPathObserver(observableMethod, sourceValue, path, memberFlags, request.HasStablePath, request.Optional);
        }

        #endregion
    }
}