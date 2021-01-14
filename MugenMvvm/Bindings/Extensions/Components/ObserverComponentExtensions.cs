using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Observation.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class ObserverComponentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberObserver TryGetMemberObserver(this ItemOrArray<IMemberObserverProviderComponent> components, IObservationManager observationManager, Type type,
            object member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            foreach (var c in components)
            {
                var observer = c.TryGetMemberObserver(observationManager, type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMemberPath? TryGetMemberPath(this ItemOrArray<IMemberPathProviderComponent> components, IObservationManager observationManager, object path,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(path, nameof(path));
            foreach (var c in components)
            {
                var memberPath = c.TryGetMemberPath(observationManager, path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMemberPathObserver? TryGetMemberPathObserver(this ItemOrArray<IMemberPathObserverProviderComponent> components, IObservationManager observationManager,
            object target,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                var observer = c.TryGetMemberPathObserver(observationManager, target, request, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }
    }
}