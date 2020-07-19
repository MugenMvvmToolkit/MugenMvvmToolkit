using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class ObserverComponentExtensions
    {
        #region Methods

        public static MemberObserver TryGetMemberObserver(this IMemberObserverProviderComponent[] components, IObservationManager observationManager, Type type, object member, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(member, nameof(member));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberObserver(observationManager, type, member, metadata);
                if (!observer.IsEmpty)
                    return observer;
            }

            return default;
        }

        public static IMemberPath? TryGetMemberPath(this IMemberPathProviderComponent[] components, IObservationManager observationManager, object path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(path, nameof(path));
            for (var i = 0; i < components.Length; i++)
            {
                var memberPath = components[i].TryGetMemberPath(observationManager, path, metadata);
                if (memberPath != null)
                    return memberPath;
            }

            return null;
        }

        public static IMemberPathObserver? TryGetMemberPathObserver(this IMemberPathObserverProviderComponent[] components, IObservationManager observationManager, object target,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(observationManager, nameof(observationManager));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var observer = components[i].TryGetMemberPathObserver(observationManager, target, request, metadata);
                if (observer != null)
                    return observer;
            }

            return null;
        }

        #endregion
    }
}